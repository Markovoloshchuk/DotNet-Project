using FluentValidation;
using MongoDB.Bson;

namespace Feedback.Application.PatientReviews.Commands.UpdatePatientReview;

public sealed class UpdatePatientReviewCommandValidator
    : AbstractValidator<UpdatePatientReviewCommand>
{
    public UpdatePatientReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => ObjectId.TryParse(id, out _))
            .WithMessage("Id must be a valid MongoDB ObjectId.");

        RuleFor(x => x.ExpectedVersion)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.Comment)
            .NotEmpty()
            .MaximumLength(2000);
    }
}