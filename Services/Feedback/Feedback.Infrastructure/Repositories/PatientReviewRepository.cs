using Feedback.Domain.Entities;
using Feedback.Domain.Interfaces;
using Feedback.Infrastructure.Settings;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace Feedback.Infrastructure.Repositories;

public sealed class PatientReviewRepository
    : IPatientReviewRepository
{
    private readonly IMongoCollection<PatientReview> _collection;

    public PatientReviewRepository(
        IMongoDatabase database,
        IOptions<FeedbackDatabaseSettings> options)
    {
        _collection = database.GetCollection<PatientReview>(
            options.Value.PatientReviewsCollection);
    }

    public Task<PatientReview?> GetByIdAsync(
        ObjectId id,
        CancellationToken ct = default)
    {
        return _collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync(ct);
    }

    public Task<bool> ExistsForPatientAndDoctorAsync(
        long patientId,
        long doctorId,
        CancellationToken ct = default)
    {
        return _collection
            .Find(x =>
                x.PatientId == patientId &&
                x.DoctorId == doctorId)
            .AnyAsync(ct);
    }

    public async Task AddAsync(
        PatientReview review,
        CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(
            review,
            cancellationToken: ct);
    }

    public async Task<bool> UpdateAsync(
        PatientReview review,
        CancellationToken ct = default)
    {
        var expectedVersion = review.Version;

        review.BumpVersion();

        var filter = Builders<PatientReview>.Filter.And(
            Builders<PatientReview>.Filter.Eq(
                x => x.Id,
                review.Id),
            Builders<PatientReview>.Filter.Eq(
                x => x.Version,
                expectedVersion));

        var result = await _collection.ReplaceOneAsync(
            filter,
            review,
            cancellationToken: ct);

        return result.MatchedCount == 1;
    }

    public async Task<bool> DeleteAsync(
        ObjectId id,
        int expectedVersion,
        CancellationToken ct = default)
    {
        var filter = Builders<PatientReview>.Filter.And(
            Builders<PatientReview>.Filter.Eq(
                x => x.Id,
                id),
            Builders<PatientReview>.Filter.Eq(
                x => x.Version,
                expectedVersion));

        var result = await _collection.DeleteOneAsync(
            filter,
            ct);

        return result.DeletedCount == 1;
    }
}   