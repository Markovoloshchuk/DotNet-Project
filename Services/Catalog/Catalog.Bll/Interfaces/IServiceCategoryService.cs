
using Catalog.Bll.DTOs.Categories;

namespace Catalog.Bll.Interfaces;

public interface IServiceCategoryService
{
    Task<IReadOnlyList<ServiceCategoryDto>> GetAllAsync(
        CancellationToken ct = default);

    Task<ServiceCategoryDto> GetByIdAsync(
        long id,
        CancellationToken ct = default);

    Task<ServiceCategoryDto> CreateAsync(
        CreateServiceCategoryDto dto,
        CancellationToken ct = default);

    Task<ServiceCategoryDto> UpdateAsync(
        long id,
        UpdateServiceCategoryDto dto,
        CancellationToken ct = default);

    Task DeleteAsync(
        long id,
        CancellationToken ct = default);
}
