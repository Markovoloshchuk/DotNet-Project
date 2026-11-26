using Feedback.Application.Common.Interfaces;
using Feedback.Application.Common.Models;
using Feedback.Domain.Entities;
using Feedback.Infrastructure.Settings;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace Feedback.Infrastructure.Queries;

public sealed class PatientReviewQueries
    : IPatientReviewQueries
{
    private readonly IMongoCollection<PatientReview> _collection;

    public PatientReviewQueries(
        IMongoDatabase database,
        IOptions<FeedbackDatabaseSettings> options)
    {
        _collection = database.GetCollection<PatientReview>(
            options.Value.PatientReviewsCollection);
    }

    public async Task<PatientReviewDto?> GetByIdAsync(
        string id,
        CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return null;

        var review = await _collection
            .Find(x => x.Id == objectId)
            .FirstOrDefaultAsync(ct);

        return review is null ? null : MapToDto(review);
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
                x => x.Rating.Value,
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

        var reviews = await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(ct);

        var items = reviews
            .Select(MapToDto)
            .ToList();

        return new PagedResult<PatientReviewDto>(
            items,
            page,
            pageSize,
            totalCount);
    }

    private static PatientReviewDto MapToDto(
        PatientReview review)
    {
        return new PatientReviewDto(
            review.Id.ToString(),
            review.ReviewId,
            review.PatientId,
            review.DoctorId,
            review.Rating.Value,
            review.Comment,
            review.CreatedAt,
            review.UpdatedAt,
            review.ClinicResponse?.Message,
            review.HelpfulVotes,
            review.VerifiedVisit,
            review.Tags.ToArray(),
            review.Version);
    }
}