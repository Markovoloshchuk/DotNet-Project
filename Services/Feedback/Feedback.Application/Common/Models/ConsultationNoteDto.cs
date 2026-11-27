namespace Feedback.Application.Common.Models;

public sealed record ConsultationNoteDto(
    string Id,
    long NoteId,
    long AppointmentId,
    string Summary,
    IReadOnlyCollection<string> Recommendations,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int Version);