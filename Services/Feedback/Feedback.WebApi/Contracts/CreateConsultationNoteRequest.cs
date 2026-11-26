namespace Feedback.WebApi.Contracts;

public sealed record CreateConsultationNoteRequest(
    long NoteId,
    long AppointmentId,
    string Summary,
    IReadOnlyCollection<string>? Recommendations);