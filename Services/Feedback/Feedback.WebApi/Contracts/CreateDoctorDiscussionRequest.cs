namespace Feedback.WebApi.Contracts;

public sealed record CreateDoctorDiscussionRequest(
    long DiscussionId,
    string Title,
    long AuthorDoctorId,
    string Content,
    IReadOnlyCollection<string>? Tags);