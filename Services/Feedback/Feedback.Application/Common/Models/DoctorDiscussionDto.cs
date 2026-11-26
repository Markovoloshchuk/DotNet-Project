namespace Feedback.Application.Common.Models;

public sealed record DiscussionCommentDto(
    long CommentId,
    long DoctorRefId,
    string Text,
    DateTime CreatedAt);

public sealed record DoctorDiscussionDto(
    string Id,
    long DiscussionId,
    string Title,
    long AuthorDoctorId,
    string Content,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<DiscussionCommentDto> Comments,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int Version);