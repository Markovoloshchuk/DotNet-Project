
using Catalog.Bll.DTOs.MedicalServices;

namespace Catalog.Bll.Interfaces;

public interface IMedicalServiceService
{
    Task<IReadOnlyList<MedicalServiceDto>> GetAllAsync(
        CancellationToken ct = default);

    Task<MedicalServiceDto> GetByIdAsync(
        long id,
        CancellationToken ct = default);

    Task<IReadOnlyList<MedicalServiceDto>> SearchAsync(
        SearchMedicalServicesDto filter,
        CancellationToken ct = default);

    Task<IReadOnlyList<MedicalServiceDto>> GetTopByPriceAsync(
        int count,
        CancellationToken ct = default);

    Task<MedicalServiceDto> CreateAsync(
        CreateMedicalServiceDto dto,
        CancellationToken ct = default);

    Task<MedicalServiceDto> UpdateAsync(
        long id,
        UpdateMedicalServiceDto dto,
        CancellationToken ct = default);

    Task DeleteAsync(
        long id,
        CancellationToken ct = default);
}
