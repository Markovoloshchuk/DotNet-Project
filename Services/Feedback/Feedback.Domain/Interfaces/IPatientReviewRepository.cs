using Feedback.Domain.Entities;

namespace Feedback.Domain.Interfaces;

public interface IPatientReviewRepository
{
    Task<PatientReview?> GetByIdAsync(
        string id,
        CancellationToken ct = default);

    Task<bool> ExistsForPatientAndDoctorAsync(
        long patientId,
        long doctorId,
        CancellationToken ct = default);

    Task AddAsync(
        PatientReview review,
        CancellationToken ct = default);

    Task<bool> UpdateAsync(
        PatientReview review,
        CancellationToken ct = default);

    Task<bool> DeleteAsync(
        string id,
        int expectedVersion,
        CancellationToken ct = default);
}