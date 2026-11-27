using Feedback.Application.Common.Validation;
using FluentValidation;

namespace Feedback.Application.PatientReviews.Commands.UpdatePatientReview;

public sealed class UpdatePatientReviewCommandValidator
    : AbstractValidator<UpdatePatientReviewCommand>
{
    public UpdatePatientReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .Must(DocumentId.IsValid)
            .WithMessage("Invalid document ID.");

        RuleFor(x => x.ExpectedVersion)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.Comment)
            .NotEmpty()
            .MaximumLength(2000);
    }
}