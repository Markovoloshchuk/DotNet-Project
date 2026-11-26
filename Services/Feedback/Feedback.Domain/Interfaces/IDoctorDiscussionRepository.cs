using Feedback.Domain.Entities;
using MongoDB.Bson;

namespace Feedback.Domain.Interfaces;

public interface IDoctorDiscussionRepository
{
    Task<DoctorDiscussion?> GetByIdAsync(
        ObjectId id,
        CancellationToken ct = default);

    Task AddAsync(
        DoctorDiscussion discussion,
        CancellationToken ct = default);

    Task<bool> UpdateAsync(
        DoctorDiscussion discussion,
        CancellationToken ct = default);

    Task<bool> DeleteAsync(
        ObjectId id,
        int expectedVersion,
        CancellationToken ct = default);
}