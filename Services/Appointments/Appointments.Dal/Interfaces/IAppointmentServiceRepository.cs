using Appointments.Domain.Entities;

namespace Appointments.Dal.Interfaces;

public interface IAppointmentServiceRepository
    : IGenericRepository<AppointmentService>
{
    Task<IReadOnlyList<AppointmentService>> GetByAppointmentIdAsync(
        long appointmentId,
        CancellationToken ct = default
    );
}