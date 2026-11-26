using Feedback.Application.Common.Models;
using Feedback.Domain.Entities;

namespace Feedback.Application.Common.Mappings;

public static class ConsultationNoteMapping
{
    public static ConsultationNoteDto ToDto(
        this ConsultationNote note)
    {
        return new ConsultationNoteDto(
            note.Id.ToString(),
            note.NoteId,
            note.AppointmentId,
            note.Summary,
            note.RecommendationsList.ToArray(),
            note.CreatedAt,
            note.UpdatedAt,
            note.Version);
    }
}