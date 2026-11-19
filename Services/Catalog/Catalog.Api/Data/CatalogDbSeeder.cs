using Catalog.Dal.Persistence;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Data;

public sealed class CatalogDbSeeder
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<CatalogDbSeeder> _logger;

    private static readonly (string Name, string? Description)[]
        CategorySeeds =
        [
            ("Diagnostics", "Diagnostic medical services."),
            ("Laboratory Tests", "Laboratory and blood tests."),
            ("Consultations", "Medical consultations with specialists."),
            ("Physiotherapy", "Physiotherapy and rehabilitation services.")
        ];

    private static readonly (
        string Name,
        string SerialNumber,
        DateOnly PurchaseDate)[]
        EquipmentSeeds =
        [
            (
                "Ultrasound Scanner",
                "US-2024-001",
                new DateOnly(2024, 1, 15)
            ),
            (
                "Electrocardiograph",
                "ECG-2024-001",
                new DateOnly(2024, 3, 10)
            ),
            (
                "Digital X-Ray System",
                "XR-2024-001",
                new DateOnly(2024, 5, 20)
            )
        ];

    private static readonly MedicalServiceSeed[] MedicalServiceSeeds =
    [
        new(
            "Diagnostics",
            "US-2024-001",
            "Abdominal Ultrasound",
            850m,
            20
        ),
        new(
            "Diagnostics",
            "US-2024-001",
            "Thyroid Ultrasound",
            650m,
            15
        ),
        new(
            "Diagnostics",
            "ECG-2024-001",
            "Electrocardiography (ECG)",
            450m,
            15
        ),
        new(
            "Diagnostics",
            "XR-2024-001",
            "Chest X-Ray",
            700m,
            10
        ),
        new(
            "Laboratory Tests",
            null,
            "Complete Blood Count",
            350m,
            15
        ),
        new(
            "Laboratory Tests",
            null,
            "Blood Glucose Test",
            200m,
            10
        ),
        new(
            "Consultations",
            null,
            "General Practitioner Consultation",
            600m,
            30
        ),
        new(
            "Consultations",
            null,
            "Cardiologist Consultation",
            900m,
            40
        ),
        new(
            "Physiotherapy",
            null,
            "Therapeutic Massage",
            750m,
            30
        )
    ];

    public CatalogDbSeeder(
        CatalogDbContext context,
        ILogger<CatalogDbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(
        CancellationToken ct = default)
    {
        var categoriesAdded = await SeedCategoriesAsync(ct);

        var equipmentAdded = await SeedEquipmentAsync(ct);

        var servicesAdded = await SeedMedicalServicesAsync(ct);

        _logger.LogInformation(
            "Catalog database seeding completed. " +
            "Categories added: {CategoriesAdded}, " +
            "equipment added: {EquipmentAdded}, " +
            "medical services added: {ServicesAdded}.",
            categoriesAdded,
            equipmentAdded,
            servicesAdded);
    }

    private async Task<int> SeedCategoriesAsync(
        CancellationToken ct)
    {
        var existingCategories = await _context.ServiceCategories
            .ToListAsync(ct);

        var categoriesByName =
            new Dictionary<string, ServiceCategory>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var category in existingCategories)
        {
            categoriesByName.TryAdd(
                category.CategoryName,
                category);
        }

        var addedCount = 0;

        foreach (var seed in CategorySeeds)
        {
            if (categoriesByName.ContainsKey(seed.Name))
            {
                continue;
            }

            var category = new ServiceCategory(
                seed.Name,
                seed.Description);

            await _context.ServiceCategories.AddAsync(
                category,
                ct);

            categoriesByName.Add(seed.Name, category);
            addedCount++;
        }

        if (addedCount > 0)
        {
            await _context.SaveChangesAsync(ct);
        }

        return addedCount;
    }

    private async Task<int> SeedEquipmentAsync(
        CancellationToken ct)
    {
        var existingEquipment = await _context.Equipment
            .ToListAsync(ct);

        var equipmentBySerialNumber =
            new Dictionary<string, Equipment>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var equipment in existingEquipment)
        {
            equipmentBySerialNumber.TryAdd(
                equipment.SerialNumber,
                equipment);
        }

        var addedCount = 0;

        foreach (var seed in EquipmentSeeds)
        {
            if (equipmentBySerialNumber.ContainsKey(
                seed.SerialNumber))
            {
                continue;
            }

            var equipment = new Equipment(
                seed.Name,
                seed.SerialNumber,
                seed.PurchaseDate);

            await _context.Equipment.AddAsync(
                equipment,
                ct);

            equipmentBySerialNumber.Add(
                seed.SerialNumber,
                equipment);

            addedCount++;
        }

        if (addedCount > 0)
        {
            await _context.SaveChangesAsync(ct);
        }

        return addedCount;
    }

    private async Task<int> SeedMedicalServicesAsync(
        CancellationToken ct)
    {
        var categories = await _context.ServiceCategories
            .ToDictionaryAsync(
                category => category.CategoryName,
                StringComparer.OrdinalIgnoreCase,
                ct);

        var equipment = await _context.Equipment
            .ToDictionaryAsync(
                item => item.SerialNumber,
                StringComparer.OrdinalIgnoreCase,
                ct);

        var existingServices = await _context.MedicalServices
            .AsNoTracking()
            .Select(service => new
            {
                service.CategoryId,
                service.ServiceName
            })
            .ToListAsync(ct);

        var existingServiceKeys = existingServices
            .Select(service => BuildServiceKey(
                service.CategoryId,
                service.ServiceName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var addedCount = 0;

        foreach (var seed in MedicalServiceSeeds)
        {
            if (!categories.TryGetValue(
                seed.CategoryName,
                out var category))
            {
                throw new InvalidOperationException(
                    $"Seed category '{seed.CategoryName}' was not found.");
            }

            var serviceKey = BuildServiceKey(
                category.CategoryId,
                seed.ServiceName);

            if (!existingServiceKeys.Add(serviceKey))
            {
                continue;
            }

            long? equipmentId = null;

            if (seed.EquipmentSerialNumber is not null)
            {
                if (!equipment.TryGetValue(
                    seed.EquipmentSerialNumber,
                    out var selectedEquipment))
                {
                    throw new InvalidOperationException(
                        "Seed equipment with serial number " +
                        $"'{seed.EquipmentSerialNumber}' was not found.");
                }

                equipmentId = selectedEquipment.EquipmentId;
            }

            var medicalService = new MedicalService(
                category.CategoryId,
                seed.ServiceName,
                seed.Price,
                seed.DurationMinutes,
                equipmentId);

            await _context.MedicalServices.AddAsync(
                medicalService,
                ct);

            addedCount++;
        }

        if (addedCount > 0)
        {
            await _context.SaveChangesAsync(ct);
        }

        return addedCount;
    }

    private static string BuildServiceKey(
        long categoryId,
        string serviceName)
    {
        return $"{categoryId}|{serviceName}";
    }

    private sealed record MedicalServiceSeed(
        string CategoryName,
        string? EquipmentSerialNumber,
        string ServiceName,
        decimal Price,
        int DurationMinutes);
}