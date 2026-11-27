
using Feedback.Application.Common.Interfaces;
using Feedback.Application.Common.Models;
using Feedback.Domain.Entities;
using Feedback.Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Feedback.Infrastructure.Queries;

public sealed class PatientReviewQueries : IPatientReviewQueries
{
    private readonly IMongoCollection<PatientReview> _collection;

    private static readonly BsonDocument ReviewProjection = new()
    {
        { "Id", new BsonDocument("$toString", "$_id") },
        { "ReviewId", "$reviewId" },
        { "PatientId", "$patientId" },
        { "DoctorId", "$doctorId" },
        { "Rating", "$rating" },
        { "Comment", "$comment" },
        { "CreatedAt", "$createdAt" },
        { "UpdatedAt", "$updatedAt" },
        {
            "ClinicResponse",
            new BsonDocument("$ifNull", new BsonArray
            {
                "$clinicResponse.message",
                BsonNull.Value
            })
        },
        { "HelpfulVotes", "$helpfulVotes" },
        { "VerifiedVisit", "$verifiedVisit" },
        { "Tags", "$tags" },
        { "Version", "$version" }
    };

    public PatientReviewQueries(MongoDbContext context)
    {
        _collection = context.PatientReviews;
    }

    public async Task<PatientReviewDto?> GetByIdAsync(
        string id,
        CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return null;

        var document = await _collection
            .Aggregate()
            .Match(x => x.Id == id)
            .Project<BsonDocument>(ReviewProjection)
            .FirstOrDefaultAsync(ct);

        return document is null
            ? null
            : MapToDto(document);
    }

    public async Task<PagedResult<PatientReviewDto>> GetListAsync(
        long? patientId,
        long? doctorId,
        int? rating,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page));

        if (pageSize is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        var builder = Builders<PatientReview>.Filter;

        var filters = new List<FilterDefinition<PatientReview>>();

        if (patientId.HasValue)
        {
            filters.Add(builder.Eq(
                x => x.PatientId,
                patientId.Value));
        }

        if (doctorId.HasValue)
        {
            filters.Add(builder.Eq(
                x => x.DoctorId,
                doctorId.Value));
        }

        if (rating.HasValue)
        {
            filters.Add(builder.Eq(
                "rating",
                rating.Value));
        }

        var filter = filters.Count == 0
            ? builder.Empty
            : builder.And(filters);

        var totalCount = await _collection.CountDocumentsAsync(
            filter,
            cancellationToken: ct);

        var skip = (int)Math.Min(
            (long)(page - 1) * pageSize,
            int.MaxValue);

        var documents = await _collection
            .Aggregate()
            .Match(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Limit(pageSize)
            .Project<BsonDocument>(ReviewProjection)
            .ToListAsync(ct);

        var items = documents
            .Select(MapToDto)
            .ToList();

        return new PagedResult<PatientReviewDto>(
            items,
            page,
            pageSize,
            totalCount);
    }

    private static PatientReviewDto MapToDto(BsonDocument document)
    {
        var updatedAtValue = document.GetValue(
            "UpdatedAt",
            BsonNull.Value);

        var clinicResponseValue = document.GetValue(
            "ClinicResponse",
            BsonNull.Value);

        var verifiedVisitValue = document.GetValue(
            "VerifiedVisit",
            BsonNull.Value);

        var tagsValue = document.GetValue(
            "Tags",
            new BsonArray());

        DateTime? updatedAt = updatedAtValue.IsBsonNull
            ? null
            : updatedAtValue.AsBsonDateTime.ToUniversalTime();

        string? clinicResponse = clinicResponseValue.IsBsonNull
            ? null
            : clinicResponseValue.AsString;

        bool? verifiedVisit = verifiedVisitValue.IsBsonNull
            ? null
            : verifiedVisitValue.AsBoolean;

        var tags = tagsValue.AsBsonArray
            .Select(x => x.AsString)
            .ToArray();

        return new PatientReviewDto(
            document["Id"].AsString,
            document["ReviewId"].ToInt64(),
            document["PatientId"].ToInt64(),
            document["DoctorId"].ToInt64(),
            document["Rating"].ToInt32(),
            document["Comment"].AsString,
            document["CreatedAt"].AsBsonDateTime.ToUniversalTime(),
            updatedAt,
            clinicResponse,
            document["HelpfulVotes"].ToInt32(),
            verifiedVisit,
            tags,
            document["Version"].ToInt32());
    }
}
