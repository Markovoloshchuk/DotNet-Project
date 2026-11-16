
using Catalog.Domain.Entities;

namespace Catalog.Dal.Interfaces;

public interface IMedicalServiceRepository
{
    // Eager Loading: отримання послуг разом із пов'язаними даними.
    Task<IReadOnlyList<MedicalService>> GetAllWithDetailsAsync(
        CancellationToken ct = default);

    Task<MedicalService?> GetByIdWithDetailsAsync(
        long serviceId,
        CancellationToken ct = default);

    // Explicit Loading: завантаження навігаційних властивостей окремо.
    Task<MedicalService?> GetByIdWithExplicitLoadingAsync(
        long serviceId,
        CancellationToken ct = default);

    // LINQ: пошук за набором умов.
    Task<IReadOnlyList<MedicalService>> SearchAsync(
        string? name = null,
        long? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? isActive = null,
        CancellationToken ct = default);

    // LINQ: отримання найдорожчих послуг.
    Task<IReadOnlyList<MedicalService>> GetTopByPriceAsync(
        int count,
        CancellationToken ct = default);
}
