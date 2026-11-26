using FluentValidation;

namespace Feedback.Application.PatientReviews.Queries.GetPatientReviewsList;

public sealed class GetPatientReviewsListQueryValidator
    : AbstractValidator<GetPatientReviewsListQuery>
{
    public GetPatientReviewsListQueryValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .When(x => x.PatientId.HasValue);

        RuleFor(x => x.DoctorId)
            .GreaterThan(0)
            .When(x => x.DoctorId.HasValue);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}