using Appointments.Bll.DTOs.Patients;

namespace Appointments.Bll.Interfaces;

public interface IPatientService
{
    Task<PatientDto> GetByIdAsync(
        long id,
        CancellationToken ct = default);

    Task<IReadOnlyList<PatientDto>> GetAllAsync(
        CancellationToken ct = default);

    Task<PatientDto> CreateAsync(
        CreatePatientDto dto,
        CancellationToken ct = default);

    Task<PatientDto> UpdateAsync(
        long id,
        UpdatePatientDto dto,
        CancellationToken ct = default);

    Task DeleteAsync(
        long id,
        CancellationToken ct = default);
}