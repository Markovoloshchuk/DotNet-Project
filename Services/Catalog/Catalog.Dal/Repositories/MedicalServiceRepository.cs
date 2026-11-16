using Catalog.Dal.Interfaces;
using Catalog.Dal.Models;
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

    // EAGER LOADING

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

    // EXPLICIT LOADING

    public async Task<MedicalService?>
        GetByIdWithExplicitLoadingAsync(
            long serviceId,
            CancellationToken ct = default)
    {
        var service = await _context.MedicalServices
            .FirstOrDefaultAsync(
                item => item.ServiceId == serviceId,
                ct);

        if (service is null)
        {
            return null;
        }

        var entry = _context.Entry(service);

        await entry
            .Reference(item => item.Category)
            .LoadAsync(ct);

        await entry
            .Reference(item => item.Equipment)
            .LoadAsync(ct);

        await entry
            .Reference(item => item.Detail)
            .LoadAsync(ct);

        await entry
            .Collection(item => item.PackageServices)
            .Query()
            .Include(packageService => packageService.Package)
            .LoadAsync(ct);

        return service;
    }

    // FILTERING, SORTING AND PAGINATION

    public async Task<PagedResult<MedicalService>> SearchAsync(
        string? name = null,
        long? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "serviceName",
        bool sortDescending = false,
        CancellationToken ct = default)
    {
        // Validate pagination parameters.
        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                "Page number must be greater than zero.");
        }

        if (pageSize <= 0 || pageSize > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Page size must be between 1 and 100.");
        }

        if (pageNumber > int.MaxValue / pageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                "Requested page number is too large.");
        }

        // Validate filters.
        if (categoryId.HasValue && categoryId.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(categoryId));
        }

        if (minPrice.HasValue && minPrice.Value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minPrice));
        }

        if (maxPrice.HasValue && maxPrice.Value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxPrice));
        }

        if (minPrice.HasValue &&
            maxPrice.HasValue &&
            minPrice.Value > maxPrice.Value)
        {
            throw new ArgumentException(
                "Minimum price cannot exceed maximum price.");
        }

        // Build the base query.
        IQueryable<MedicalService> query =
            _context.MedicalServices
                .AsNoTracking()
                .Include(service => service.Category)
                .Include(service => service.Equipment)
                .Include(service => service.Detail)
                .Include(service => service.PackageServices)
                    .ThenInclude(packageService => packageService.Package);

        // Apply filters.
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

        // Count records before pagination.
        var totalCount = await query.CountAsync(ct);

        // Validate and normalize sorting field.
        var sortField = string.IsNullOrWhiteSpace(sortBy)
            ? "servicename"
            : sortBy.Trim().ToLowerInvariant();

        // Apply sorting.
        IOrderedQueryable<MedicalService> orderedQuery =
            sortField switch
            {
                "servicename" => sortDescending
                    ? query.OrderByDescending(s => s.ServiceName)
                    : query.OrderBy(s => s.ServiceName),

                "price" => sortDescending
                    ? query.OrderByDescending(s => s.Price)
                    : query.OrderBy(s => s.Price),

                "durationminutes" => sortDescending
                    ? query.OrderByDescending(s => s.DurationMinutes)
                    : query.OrderBy(s => s.DurationMinutes),

                "isactive" => sortDescending
                    ? query.OrderByDescending(s => s.IsActive)
                    : query.OrderBy(s => s.IsActive),

                "serviceid" => sortDescending
                    ? query.OrderByDescending(s => s.ServiceId)
                    : query.OrderBy(s => s.ServiceId),

                _ => throw new ArgumentException(
                    "Unsupported sort field. Allowed values: " +
                    "serviceName, price, durationMinutes, " +
                    "isActive, serviceId.",
                    nameof(sortBy))
            };

        // Add a stable secondary order for consistent pagination.
        if (sortField != "serviceid")
        {
            orderedQuery = orderedQuery.ThenBy(
                service => service.ServiceId);
        }

        // Apply pagination in the database query.
        var skip = (pageNumber - 1) * pageSize;

        var items = await orderedQuery
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<MedicalService>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    // TOP SERVICES BY PRICE

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