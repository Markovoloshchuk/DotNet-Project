using Feedback.Application.Common.Exceptions;
using Feedback.Domain.Interfaces;
using MediatR;

namespace Feedback.Application.PatientReviews.Commands.UpdatePatientReview;

public sealed class UpdatePatientReviewCommandHandler(
    IPatientReviewRepository repository)
    : IRequestHandler<UpdatePatientReviewCommand, bool>
{
    public async Task<bool> Handle(
        UpdatePatientReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (review is null)
            throw new NotFoundException(
                "Patient review was not found.");

        if (review.Version != request.ExpectedVersion)
            return false;

        review.UpdateRating(request.Rating);
        review.UpdateComment(request.Comment);

        return await repository.UpdateAsync(
            review,
            cancellationToken);
    }
}