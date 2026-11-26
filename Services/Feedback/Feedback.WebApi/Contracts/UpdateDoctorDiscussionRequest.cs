namespace Feedback.WebApi.Contracts;

public sealed record UpdateDoctorDiscussionRequest(
    string Title,
    string Content,
    IReadOnlyCollection<string>? TagsToAdd);