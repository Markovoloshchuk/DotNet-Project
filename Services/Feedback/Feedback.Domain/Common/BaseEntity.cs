using MongoDB.Bson;

namespace Feedback.Domain.Common;

public abstract class BaseEntity
{
    public ObjectId Id { get; protected set; } = ObjectId.GenerateNewId();

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

    protected void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void BumpVersion()
    {
        Version++;
    }
}