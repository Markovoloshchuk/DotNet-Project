using System.Diagnostics;
using Feedback.Application.Common.Exceptions;
using Feedback.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Feedback.Application.PatientReviews.Commands.UpdatePatientReview;

public sealed class UpdatePatientReviewCommandHandler(
    IPatientReviewRepository repository,
    ILogger<UpdatePatientReviewCommandHandler> logger)
    : IRequestHandler<UpdatePatientReviewCommand, bool>
{
    private static readonly ActivitySource ActivitySource =
        new("Clinic.Feedback");

    public async Task<bool> Handle(
        UpdatePatientReviewCommand request,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "Feedback.PatientReview.Update");

        activity?.SetTag("feedback.review.id", request.Id);
        activity?.SetTag(
            "feedback.review.expected_version",
            request.ExpectedVersion);

        try
        {
            var review = await repository.GetByIdAsync(
                request.Id,
                cancellationToken);

            if (review is null)
            {
                activity?.SetStatus(
                    ActivityStatusCode.Error,
                    "Patient review was not found");

                logger.LogWarning(
                    "Cannot update patient review {ReviewId}: review was not found",
                    request.Id);

                throw new NotFoundException(
                    "Patient review was not found.");
            }

            if (review.Version != request.ExpectedVersion)
            {
                activity?.SetStatus(
                    ActivityStatusCode.Error,
                    "Patient review version conflict");

                logger.LogWarning(
                    "Version conflict while updating patient review {ReviewId}. Expected version {ExpectedVersion}, actual version {ActualVersion}",
                    request.Id,
                    request.ExpectedVersion,
                    review.Version);

                return false;
            }

            review.UpdateRating(request.Rating);
            review.UpdateComment(request.Comment);

            var updated = await repository.UpdateAsync(
                review,
                cancellationToken);

            if (!updated)
            {
                activity?.SetStatus(
                    ActivityStatusCode.Error,
                    "Patient review update conflict");

                logger.LogWarning(
                    "Patient review {ReviewId} was not updated because of a concurrent modification",
                    request.Id);

                return false;
            }

            logger.LogInformation(
                "Updated patient review {ReviewId}",
                request.Id);

            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Failed to update patient review");

            // LoggingBehavior records the exception.
            throw;
        }
    }
}