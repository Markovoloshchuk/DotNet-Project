using Feedback.Application.Common.Models;

namespace Feedback.Application.Common.Interfaces;

public interface IPatientReviewQueries
{
    Task<PatientReviewDto?> GetByIdAsync(
        string id,
        CancellationToken ct = default);

    Task<PagedResult<PatientReviewDto>> GetListAsync(
        long? patientId,
        long? doctorId,
        int? rating,
        int page,
        int pageSize,
        CancellationToken ct = default);
}