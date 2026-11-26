using Feedback.Application.Common.Interfaces;
using Feedback.Domain.Entities;
using Feedback.Domain.Interfaces;
using MediatR;

namespace Feedback.Application.PatientReviews.Commands.CreatePatientReview;

public sealed class CreatePatientReviewCommandHandler(
    IPatientReviewRepository repository)
    : IRequestHandler<CreatePatientReviewCommand, string>
{
    public async Task<string> Handle(
        CreatePatientReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = new PatientReview(
            request.ReviewId,
            request.PatientId,
            request.DoctorId,
            request.Rating,
            request.Comment,
            request.VerifiedVisit,
            request.Tags);

        await repository.AddAsync(review, cancellationToken);

        return review.Id.ToString();
    }
}