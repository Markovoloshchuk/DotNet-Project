using Feedback.Domain.Common;
using Feedback.Domain.Exceptions;

namespace Feedback.Domain.Entities;

public sealed class ConsultationNote : BaseEntity
{
    private readonly List<string> _recommendationsList = new();

    public long NoteId { get; private set; }

    public long AppointmentId { get; private set; }

    public string Summary { get; private set; } = string.Empty;

    public IReadOnlyCollection<string> RecommendationsList =>
        _recommendationsList.AsReadOnly();

    private ConsultationNote()
    {
    }

    public ConsultationNote(
        long noteId,
        long appointmentId,
        string summary,
        IEnumerable<string>? recommendations = null,
        DateTime? createdAt = null)
        : base(createdAt)
    {
        if (noteId <= 0)
            throw new DomainException("NoteId must be positive.");

        if (appointmentId <= 0)
            throw new DomainException("AppointmentId must be positive.");

        if (string.IsNullOrWhiteSpace(summary))
            throw new DomainException("Summary is required.");

        NoteId = noteId;
        AppointmentId = appointmentId;
        Summary = summary.Trim();

        if (recommendations is not null)
        {
            foreach (var recommendation in recommendations)
            {
                AddRecommendationInternal(recommendation);
            }
        }
    }

    public void UpdateSummary(string summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new DomainException("Summary is required.");

        Summary = summary.Trim();
        Touch();
    }

    public void AddRecommendation(string recommendation)
    {
        AddRecommendationInternal(recommendation);
        Touch();
    }

    private void AddRecommendationInternal(string recommendation)
    {
        if (string.IsNullOrWhiteSpace(recommendation))
            throw new DomainException(
                "Recommendation cannot be empty.");

        _recommendationsList.Add(recommendation.Trim());
    }
}