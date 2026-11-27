using Feedback.Domain.Common;
using Feedback.Domain.Exceptions;

namespace Feedback.Domain.ValueObjects;

public sealed class ClinicResponse : ValueObject
{
    public DateTime RespondedAt { get; }

    public string Message { get; }

    public ClinicResponse(DateTime respondedAt, string message)
    {
        if (respondedAt == default)
        {
            throw new DomainException(
                "Response date is required.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new DomainException(
                "Response message is required.");
        }

        RespondedAt = respondedAt.ToUniversalTime();
        Message = message.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return RespondedAt;
        yield return Message;
    }
}