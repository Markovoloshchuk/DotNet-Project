
using Catalog.Dal.Interfaces;
using Catalog.Dal.Persistence;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Dal.Repositories;

public sealed class MedicalServiceRepository
    : IMedicalServiceRepository
{
    private readonly CatalogDbContext _context;

    public MedicalServiceRepository(CatalogDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    // --------------------------------------------------
    // EAGER LOADING
    // --------------------------------------------------

    public async Task<IReadOnlyList<MedicalService>>
        GetAllWithDetailsAsync(CancellationToken ct = default)
    {
        return await _context.MedicalServices
            .AsNoTracking()
            .Include(service => service.Category)
            .Include(service => service.Equipment)
            .Include(service => service.Detail)
            .Include(service => service.PackageServices)
                .ThenInclude(packageService => packageService.Package)
            .OrderBy(service => service.ServiceName)
            .ToListAsync(ct);
    }

    public async Task<MedicalService?> GetByIdWithDetailsAsync(
        long serviceId,
        CancellationToken ct = default)
    {
        return await _context.MedicalServices
            .AsNoTracking()
            .Include(service => service.Category)
            .Include(service => service.Equipment)
            .Include(service => service.Detail)
            .Include(service => service.PackageServices)
                .ThenInclude(packageService => packageService.Package)
            .FirstOrDefaultAsync(
                service => service.ServiceId == serviceId,
                ct);
    }

    // --------------------------------------------------
    // EXPLICIT LOADING
    // --------------------------------------------------

    public async Task<MedicalService?>
        GetByIdWithExplicitLoadingAsync(
            long serviceId,
            CancellationToken ct = default)
    {
        // The entity must be tracked for explicit loading.
        var service = await _context.MedicalServices
            .FirstOrDefaultAsync(
                item => item.ServiceId == serviceId,
                ct);

        if (service is null)
        {
            return null;
        }

        var entry = _context.Entry(service);

        // Load reference navigations separately.
        await entry
            .Reference(item => item.Category)
            .LoadAsync(ct);

        await entry
            .Reference(item => item.Equipment)
            .LoadAsync(ct);

        await entry
            .Reference(item => item.Detail)
            .LoadAsync(ct);

        // Load the collection and its related packages.
        await entry
            .Collection(item => item.PackageServices)
            .Query()
            .Include(packageService => packageService.Package)
            .LoadAsync(ct);

        return service;
    }

    // --------------------------------------------------
    // LINQ QUERIES
    // --------------------------------------------------

    public async Task<IReadOnlyList<MedicalService>> SearchAsync(
        string? name = null,
        long? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        if (categoryId.HasValue && categoryId.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(categoryId));
        }

        if (minPrice.HasValue && minPrice.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minPrice));
        }

        if (maxPrice.HasValue && maxPrice.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPrice));
        }

        if (minPrice.HasValue &&
            maxPrice.HasValue &&
            minPrice.Value > maxPrice.Value)
        {
            throw new ArgumentException(
                "Minimum price cannot exceed maximum price.");
        }

        IQueryable<MedicalService> query =
            _context.MedicalServices
                .AsNoTracking()
                .Include(service => service.Category);

        if (!string.IsNullOrWhiteSpace(name))
        {
            var searchName = name.Trim();

            query = query.Where(service =>
                service.ServiceName.Contains(searchName));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(service =>
                service.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(service =>
                service.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(service =>
                service.Price <= maxPrice.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(service =>
                service.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(service => service.ServiceName)
            .ThenBy(service => service.Price)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MedicalService>>
        GetTopByPriceAsync(
            int count,
            CancellationToken ct = default)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "Count must be greater than zero.");
        }

        return await _context.MedicalServices
            .AsNoTracking()
            .Include(service => service.Category)
            .OrderByDescending(service => service.Price)
            .ThenBy(service => service.ServiceName)
            .Take(count)
            .ToListAsync(ct);
    }
}
