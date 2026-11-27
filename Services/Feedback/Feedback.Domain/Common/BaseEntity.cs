using System.Security.Cryptography;

namespace Feedback.Domain.Common;

public abstract class BaseEntity
{
    public string Id { get; protected set; } = GenerateId();

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; protected set; }

    public int Version { get; protected set; }

    protected BaseEntity()
    {
    }

    protected BaseEntity(DateTime? createdAt)
    {
        CreatedAt = createdAt?.ToUniversalTime() ?? DateTime.UtcNow;
    }

    private static string GenerateId()
    {
        return Convert.ToHexString(
            RandomNumberGenerator.GetBytes(12)
        ).ToLowerInvariant();
    }

    protected void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void BumpVersion()
    {
        Version++;
    }
}