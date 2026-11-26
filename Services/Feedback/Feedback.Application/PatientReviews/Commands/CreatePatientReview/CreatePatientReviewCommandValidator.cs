using FluentValidation;

namespace Feedback.Application.PatientReviews.Commands.CreatePatientReview;

public sealed class CreatePatientReviewCommandValidator
    : AbstractValidator<CreatePatientReviewCommand>
{
    public CreatePatientReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).GreaterThan(0);
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.DoctorId).GreaterThan(0);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.Comment)
            .NotEmpty()
            .MaximumLength(2000);

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .MaximumLength(100);
    }
}