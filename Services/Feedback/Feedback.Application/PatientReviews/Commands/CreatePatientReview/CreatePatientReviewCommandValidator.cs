using Feedback.Domain.Interfaces;
using FluentValidation;

namespace Feedback.Application.PatientReviews.Commands.CreatePatientReview;

public sealed class CreatePatientReviewCommandValidator
    : AbstractValidator<CreatePatientReviewCommand>
{
    public CreatePatientReviewCommandValidator(
        IPatientReviewRepository repository)
    {
        RuleFor(x => x.ReviewId)
            .GreaterThan(0);

        RuleFor(x => x.PatientId)
            .GreaterThan(0);

        RuleFor(x => x.DoctorId)
            .GreaterThan(0);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.Comment)
            .NotEmpty()
            .MaximumLength(2000);

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
                !await repository.ExistsForPatientAndDoctorAsync(
                    command.PatientId,
                    command.DoctorId,
                    cancellationToken))
            .When(command =>
                command.PatientId > 0 &&
                command.DoctorId > 0)
            .WithMessage(
                "This patient has already submitted a review for this doctor.");
    }
}