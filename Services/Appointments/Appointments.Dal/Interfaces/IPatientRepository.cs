using Appointments.Domain.Entities;

namespace Appointments.Dal.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(long id, CancellationToken ct = default);

    Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken ct = default);

    Task<long> AddAsync(Patient patient, CancellationToken ct = default);

    Task<bool> UpdateAsync(Patient patient, CancellationToken ct = default);

    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}