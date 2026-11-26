using Feedback.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Feedback.Infrastructure.Persistence;

public sealed class RatingSerializer : SerializerBase<Rating>
{
    public override Rating Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var reader = context.Reader;

        return reader.GetCurrentBsonType() switch
        {
            BsonType.Int32 => new Rating(reader.ReadInt32()),
            BsonType.Int64 => new Rating(
                checked((int)reader.ReadInt64())),
            _ => throw new BsonSerializationException(
                "Rating must be stored as a BSON integer.")
        };
    }

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        Rating value)
    {
        context.Writer.WriteInt32(value.Value);
    }
}