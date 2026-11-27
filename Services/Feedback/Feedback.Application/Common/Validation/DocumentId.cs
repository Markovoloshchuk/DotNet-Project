namespace Feedback.Application.Common.Validation;

public static class DocumentId
{
    public static bool IsValid(string? id)
    {
        return id is { Length: 24 }
            && id.All(Uri.IsHexDigit);
    }
}