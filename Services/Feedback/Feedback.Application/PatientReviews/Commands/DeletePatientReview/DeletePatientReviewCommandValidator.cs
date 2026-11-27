using Feedback.Application.Common.Validation;
using FluentValidation;

namespace Feedback.Application.PatientReviews.Commands.DeletePatientReview;

public sealed class DeletePatientReviewCommandValidator
    : AbstractValidator<DeletePatientReviewCommand>
{
    public DeletePatientReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .Must(DocumentId.IsValid)
            .WithMessage("Invalid document ID.");

        RuleFor(x => x.ExpectedVersion)
            .GreaterThanOrEqualTo(0);
    }
}