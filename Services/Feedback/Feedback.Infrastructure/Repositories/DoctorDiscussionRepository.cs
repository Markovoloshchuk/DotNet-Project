using Feedback.Domain.Entities;
using Feedback.Domain.Interfaces;
using Feedback.Infrastructure.Settings;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace Feedback.Infrastructure.Repositories;

public sealed class DoctorDiscussionRepository
    : IDoctorDiscussionRepository
{
    private readonly IMongoCollection<DoctorDiscussion> _collection;

    public DoctorDiscussionRepository(
        IMongoDatabase database,
        IOptions<FeedbackDatabaseSettings> options)
    {
        _collection = database.GetCollection<DoctorDiscussion>(
            options.Value.DoctorDiscussionsCollection);
    }

    public Task<DoctorDiscussion?> GetByIdAsync(
        ObjectId id,
        CancellationToken ct = default)
    {
        return _collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(
        DoctorDiscussion discussion,
        CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(
            discussion,
            cancellationToken: ct);
    }

    public async Task<bool> UpdateAsync(
        DoctorDiscussion discussion,
        CancellationToken ct = default)
    {
        var expectedVersion = discussion.Version;

        discussion.BumpVersion();

        var filter = Builders<DoctorDiscussion>.Filter.And(
            Builders<DoctorDiscussion>.Filter.Eq(
                x => x.Id,
                discussion.Id),
            Builders<DoctorDiscussion>.Filter.Eq(
                x => x.Version,
                expectedVersion));

        var result = await _collection.ReplaceOneAsync(
            filter,
            discussion,
            cancellationToken: ct);

        return result.MatchedCount == 1;
    }

    public async Task<bool> DeleteAsync(
        ObjectId id,
        int expectedVersion,
        CancellationToken ct = default)
    {
        var filter = Builders<DoctorDiscussion>.Filter.And(
            Builders<DoctorDiscussion>.Filter.Eq(
                x => x.Id,
                id),
            Builders<DoctorDiscussion>.Filter.Eq(
                x => x.Version,
                expectedVersion));

        var result = await _collection.DeleteOneAsync(
            filter,
            ct);

        return result.DeletedCount == 1;
    }
}