using Feedback.Domain.Common;
using Feedback.Domain.Exceptions;

namespace Feedback.Domain.ValueObjects;

public sealed class Rating : ValueObject
{
    public int Value { get; }

    public Rating(int value)
    {
        if (value is < 1 or > 5)
        {
            throw new DomainException(
                "Rating must be between 1 and 5.");
        }

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}