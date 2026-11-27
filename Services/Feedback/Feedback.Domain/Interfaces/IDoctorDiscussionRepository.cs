using Feedback.Domain.Entities;

namespace Feedback.Domain.Interfaces;

public interface IDoctorDiscussionRepository
{
    Task<DoctorDiscussion?> GetByIdAsync(
        string id,
        CancellationToken ct = default);

    Task AddAsync(
        DoctorDiscussion discussion,
        CancellationToken ct = default);

    Task<bool> UpdateAsync(
        DoctorDiscussion discussion,
        CancellationToken ct = default);

    Task<bool> DeleteAsync(
        string id,
        int expectedVersion,
        CancellationToken ct = default);
}