namespace Feedback.WebApi.Contracts;

public sealed record AddDiscussionCommentRequest(
    long CommentId,
    long DoctorRefId,
    string Text);