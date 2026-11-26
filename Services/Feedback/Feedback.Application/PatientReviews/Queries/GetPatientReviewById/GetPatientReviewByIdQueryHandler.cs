using Feedback.Application.Common.Interfaces;
using Feedback.Application.Common.Models;
using MediatR;

namespace Feedback.Application.PatientReviews.Queries.GetPatientReviewById;

public sealed class GetPatientReviewByIdQueryHandler(
    IPatientReviewQueries queries)
    : IRequestHandler<GetPatientReviewByIdQuery, PatientReviewDto?>
{
    public Task<PatientReviewDto?> Handle(
        GetPatientReviewByIdQuery request,
        CancellationToken cancellationToken)
    {
        return queries.GetByIdAsync(request.Id, cancellationToken);
    }
}