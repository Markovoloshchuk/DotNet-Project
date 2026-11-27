using Appointments.Domain.Entities;

namespace Appointments.Dal.Interfaces;

public interface IAppointmentRepository : IGenericRepository<Appointment>
{
    Task<Appointment?> GetWithServicesAsync(
        long appointmentId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Appointment>> GetAllWithServicesAsync(
        CancellationToken ct = default);
}
