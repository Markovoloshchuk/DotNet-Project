using Feedback.Domain.Entities;

namespace Feedback.Domain.Interfaces;

public interface IConsultationNoteRepository
{
    Task<ConsultationNote?> GetByIdAsync(
        string id,
        CancellationToken ct = default);

    Task<ConsultationNote?> GetByAppointmentIdAsync(
        long appointmentId,
        CancellationToken ct = default);

    Task AddAsync(
        ConsultationNote note,
        CancellationToken ct = default);

    Task<bool> UpdateAsync(
        ConsultationNote note,
        CancellationToken ct = default);
}