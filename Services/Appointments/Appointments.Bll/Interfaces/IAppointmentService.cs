using Appointments.Bll.DTOs.Appointments;

namespace Appointments.Bll.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentDto> GetByIdAsync(
        long id,
        CancellationToken ct = default);

    Task<IReadOnlyList<AppointmentDto>> GetAllAsync(
        CancellationToken ct = default);

    Task<AppointmentDto> CreateAsync(
        CreateAppointmentDto dto,
        CancellationToken ct = default);

    Task<AppointmentDto> UpdateAsync(
        long id,
        UpdateAppointmentDto dto,
        CancellationToken ct = default);

    Task DeleteAsync(
        long id,
        CancellationToken ct = default);
}