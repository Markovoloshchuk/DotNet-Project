namespace Feedback.WebApi.Contracts;

public sealed record UpdateConsultationNoteRequest(
    string Summary,
    IReadOnlyCollection<string>? RecommendationsToAdd);