
using Catalog.Dal.Models;
using Catalog.Domain.Entities;

namespace Catalog.Dal.Interfaces;

public interface IMedicalServiceRepository
{
    // Eager Loading
    Task<IReadOnlyList<MedicalService>> GetAllWithDetailsAsync(
        CancellationToken ct = default);

    Task<MedicalService?> GetByIdWithDetailsAsync(
        long serviceId,
        CancellationToken ct = default);

    // Explicit Loading
    Task<MedicalService?> GetByIdWithExplicitLoadingAsync(
        long serviceId,
        CancellationToken ct = default);

    // LINQ M:N через проміжну сутність PackageService
    Task<IReadOnlyList<PackageService>>
        GetPackageServicesByPackageIdAsync(
            long packageId,
            int? minQuantity = null,
            CancellationToken ct = default);

    // Filtering, Sorting and Pagination
    Task<PagedResult<MedicalService>> SearchAsync(
        string? name = null,
        long? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "serviceName",
        bool sortDescending = false,
        CancellationToken ct = default);

    Task<IReadOnlyList<MedicalService>> GetTopByPriceAsync(
        int count,
        CancellationToken ct = default);
}
