using Feedback.Domain.Common;
using Feedback.Domain.Exceptions;

namespace Feedback.Domain.Entities;

public sealed class DoctorDiscussion : BaseEntity
{
    private readonly List<string> _tags = new();
    private readonly List<DiscussionComment> _comments = new();

    public long DiscussionId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public long AuthorDoctorId { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    public IReadOnlyCollection<DiscussionComment> Comments =>
        _comments.AsReadOnly();

    private DoctorDiscussion()
    {
    }

    public DoctorDiscussion(
        long discussionId,
        string title,
        long authorDoctorId,
        string content,
        IEnumerable<string>? tags = null,
        DateTime? createdAt = null)
        : base(createdAt)
    {
        if (discussionId <= 0)
            throw new DomainException("DiscussionId must be positive.");

        if (authorDoctorId <= 0)
            throw new DomainException("AuthorDoctorId must be positive.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Discussion title is required.");

        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Discussion content is required.");

        DiscussionId = discussionId;
        Title = title.Trim();
        AuthorDoctorId = authorDoctorId;
        Content = content.Trim();

        if (tags is not null)
        {
            foreach (var tag in tags)
            {
                AddTagInternal(tag);
            }
        }
    }

    public void UpdateContent(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Discussion title is required.");

        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Discussion content is required.");

        Title = title.Trim();
        Content = content.Trim();

        Touch();
    }

    public void AddComment(
        long commentId,
        long doctorRefId,
        string text)
    {
        AddComment(new DiscussionComment(
            commentId,
            doctorRefId,
            text));
    }

    public void AddComment(DiscussionComment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        if (_comments.Any(c => c.CommentId == comment.CommentId))
        {
            throw new DomainException(
                "Comment with this ID already exists.");
        }

        _comments.Add(comment);
        Touch();
    }

    public void AddTag(string tag)
    {
        AddTagInternal(tag);
        Touch();
    }

    private void AddTagInternal(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new DomainException("Tag cannot be empty.");

        var normalizedTag = tag.Trim();

        if (_tags.Any(existing =>
            string.Equals(
                existing,
                normalizedTag,
                StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _tags.Add(normalizedTag);
    }
}