using Feedback.Domain.Entities;
using Feedback.Domain.Exceptions;

namespace Feedback.Tests;

public sealed class ConsultationNoteTests
{
    [Fact]
    public void Constructor_WhenAppointmentIdIsNotPositive_ThrowsDomainException()
    {
        var action = () => new ConsultationNote(
            noteId: 1,
            appointmentId: 0,
            summary: "Consultation summary");

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void UpdateSummaryAndAddRecommendation_ModifiesNote()
    {
        var note = new ConsultationNote(
            noteId: 1001,
            appointmentId: 2001,
            summary: "Initial summary",
            recommendations: ["Initial recommendation"]);

        note.UpdateSummary("  Updated summary  ");
        note.AddRecommendation("  Additional recommendation  ");

        Assert.Equal("Updated summary", note.Summary);
        Assert.Equal(
            ["Initial recommendation", "Additional recommendation"],
            note.RecommendationsList);
        Assert.NotNull(note.UpdatedAt);
    }
}