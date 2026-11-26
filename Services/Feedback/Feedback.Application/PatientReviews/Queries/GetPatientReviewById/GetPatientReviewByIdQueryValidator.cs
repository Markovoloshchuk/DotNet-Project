using FluentValidation;
using MongoDB.Bson;

namespace Feedback.Application.PatientReviews.Queries.GetPatientReviewById;

public sealed class GetPatientReviewByIdQueryValidator
    : AbstractValidator<GetPatientReviewByIdQuery>
{
    public GetPatientReviewByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => ObjectId.TryParse(id, out _))
            .WithMessage("Id must be a valid MongoDB ObjectId.");
    }
}