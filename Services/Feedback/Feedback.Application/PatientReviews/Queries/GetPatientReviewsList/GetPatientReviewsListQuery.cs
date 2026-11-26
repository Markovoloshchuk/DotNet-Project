using Feedback.Application.Common.Interfaces;
using Feedback.Application.Common.Models;

namespace Feedback.Application.PatientReviews.Queries.GetPatientReviewsList;

public sealed record GetPatientReviewsListQuery(
    long? PatientId = null,
    long? DoctorId = null,
    int? Rating = null,
    int Page = 1,
    int PageSize = 20)
    : IQuery<PagedResult<PatientReviewDto>>;