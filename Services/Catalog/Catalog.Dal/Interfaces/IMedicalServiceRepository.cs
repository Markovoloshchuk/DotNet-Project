using Catalog.Dal.Models;
using Catalog.Domain.Entities;

namespace Catalog.Dal.Interfaces;

public interface IMedicalServiceRepository
{
    Task<IReadOnlyList<MedicalService>> GetAllWithDetailsAsync(
        CancellationToken ct = default);

    Task<MedicalService?> GetByIdWithDetailsAsync(
        long serviceId,
        CancellationToken ct = default);

    Task<MedicalService?> GetByIdWithExplicitLoadingAsync(
        long serviceId,
        CancellationToken ct = default);

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