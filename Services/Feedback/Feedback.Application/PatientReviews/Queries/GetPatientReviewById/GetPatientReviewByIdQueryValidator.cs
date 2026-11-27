using Feedback.Application.Common.Validation;
using FluentValidation;

namespace Feedback.Application.PatientReviews.Queries.GetPatientReviewById;

public sealed class GetPatientReviewByIdQueryValidator
    : AbstractValidator<GetPatientReviewByIdQuery>
{
    public GetPatientReviewByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .Must(DocumentId.IsValid)
            .WithMessage("Invalid document ID.");
    }
}