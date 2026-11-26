using Feedback.Domain.Entities;
using Feedback.Domain.Interfaces;
using Feedback.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Feedback.Infrastructure.Repositories;

public sealed class ConsultationNoteRepository
    : IConsultationNoteRepository
{
    private readonly IMongoCollection<ConsultationNote> _collection;

    public ConsultationNoteRepository(
        IMongoDatabase database,
        IOptions<FeedbackDatabaseSettings> options)
    {
        _collection = database.GetCollection<ConsultationNote>(
            options.Value.ConsultationNotesCollection);
    }

    public async Task<ConsultationNote?> GetByIdAsync(
        ObjectId id,
        CancellationToken ct = default)
    {
        return await _collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ConsultationNote?> GetByAppointmentIdAsync(
        long appointmentId,
        CancellationToken ct = default)
    {
        return await _collection
            .Find(x => x.AppointmentId == appointmentId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(
        ConsultationNote note,
        CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(
            note,
            cancellationToken: ct);
    }

    public async Task<bool> UpdateAsync(
        ConsultationNote note,
        CancellationToken ct = default)
    {
        var expectedVersion = note.Version;

        note.BumpVersion();

        var filter = Builders<ConsultationNote>.Filter.And(
            Builders<ConsultationNote>.Filter.Eq(
                x => x.Id,
                note.Id),
            Builders<ConsultationNote>.Filter.Eq(
                x => x.Version,
                expectedVersion));

        var result = await _collection.ReplaceOneAsync(
            filter,
            note,
            cancellationToken: ct);

        return result.MatchedCount == 1;
    }
}