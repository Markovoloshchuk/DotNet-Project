using Feedback.Domain.Common;
using Feedback.Domain.Exceptions;
using Feedback.Domain.ValueObjects;

namespace Feedback.Domain.Entities;

public sealed class PatientReview : BaseEntity
{
    private readonly List<string> _tags = new();

    public long ReviewId { get; private set; }

    public long PatientId { get; private set; }

    public long DoctorId { get; private set; }

    public Rating Rating { get; private set; } = null!;

    public string Comment { get; private set; } = string.Empty;

    public ClinicResponse? ClinicResponse { get; private set; }

    public int HelpfulVotes { get; private set; }

    public bool? VerifiedVisit { get; private set; }

    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    private PatientReview()
    {
    }

    public PatientReview(
        long reviewId,
        long patientId,
        long doctorId,
        int rating,
        string comment,
        bool? verifiedVisit = null,
        IEnumerable<string>? tags = null,
        DateTime? createdAt = null)
        : base(createdAt)
    {
        if (reviewId <= 0)
            throw new DomainException("ReviewId must be positive.");

        if (patientId <= 0)
            throw new DomainException("PatientId must be positive.");

        if (doctorId <= 0)
            throw new DomainException("DoctorId must be positive.");

        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("Review comment is required.");

        ReviewId = reviewId;
        PatientId = patientId;
        DoctorId = doctorId;
        Rating = new Rating(rating);
        Comment = comment.Trim();
        VerifiedVisit = verifiedVisit;

        if (tags is not null)
        {
            foreach (var tag in tags)
            {
                AddTagInternal(tag);
            }
        }
    }

    public void UpdateComment(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("Review comment is required.");

        Comment = comment.Trim();
        Touch();
    }

    public void UpdateRating(int rating)
    {
        Rating = new Rating(rating);
        Touch();
    }

    public void SetClinicResponse(
        string message,
        DateTime? respondedAt = null)
    {
        ClinicResponse = new ClinicResponse(
            respondedAt ?? DateTime.UtcNow,
            message);

        Touch();
    }

    public void AddHelpfulVote()
    {
        HelpfulVotes++;
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