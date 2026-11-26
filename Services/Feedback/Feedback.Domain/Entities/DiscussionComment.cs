using Feedback.Domain.Exceptions;

namespace Feedback.Domain.Entities;

public sealed class DiscussionComment
{
    public long CommentId { get; private set; }

    public long DoctorRefId { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    private DiscussionComment()
    {
    }

    public DiscussionComment(
        long commentId,
        long doctorRefId,
        string text,
        DateTime? createdAt = null)
    {
        if (commentId <= 0)
            throw new DomainException("CommentId must be positive.");

        if (doctorRefId <= 0)
            throw new DomainException("DoctorRefId must be positive.");

        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Comment text is required.");

        CommentId = commentId;
        DoctorRefId = doctorRefId;
        Text = text.Trim();
        CreatedAt = createdAt?.ToUniversalTime() ?? DateTime.UtcNow;
    }
}