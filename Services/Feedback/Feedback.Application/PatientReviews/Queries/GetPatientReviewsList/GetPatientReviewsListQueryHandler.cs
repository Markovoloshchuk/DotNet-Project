using Feedback.Application.Common.Interfaces;
using Feedback.Application.Common.Models;
using MediatR;

namespace Feedback.Application.PatientReviews.Queries.GetPatientReviewsList;

public sealed class GetPatientReviewsListQueryHandler(
    IPatientReviewQueries queries)
    : IRequestHandler<GetPatientReviewsListQuery, PagedResult<PatientReviewDto>>
{
    public Task<PagedResult<PatientReviewDto>> Handle(
        GetPatientReviewsListQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return queries.GetListAsync(
            request.PatientId,
            request.DoctorId,
            request.Rating,
            page,
            pageSize,
            cancellationToken);
    }
}