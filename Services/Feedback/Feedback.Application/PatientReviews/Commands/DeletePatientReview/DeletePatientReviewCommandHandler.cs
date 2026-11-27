using System.Diagnostics;
using Feedback.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Feedback.Application.PatientReviews.Commands.DeletePatientReview;

public sealed class DeletePatientReviewCommandHandler(
    IPatientReviewRepository repository,
    ILogger<DeletePatientReviewCommandHandler> logger)
    : IRequestHandler<DeletePatientReviewCommand, bool>
{
    private static readonly ActivitySource ActivitySource =
        new("Clinic.Feedback");

    public async Task<bool> Handle(
        DeletePatientReviewCommand request,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "Feedback.PatientReview.Delete");

        activity?.SetTag("feedback.review.id", request.Id);
        activity?.SetTag(
            "feedback.review.expected_version",
            request.ExpectedVersion);

        try
        {
            var deleted = await repository.DeleteAsync(
                request.Id,
                request.ExpectedVersion,
                cancellationToken);

            if (!deleted)
            {
                activity?.SetStatus(
                    ActivityStatusCode.Error,
                    "Patient review was not deleted");

                logger.LogWarning(
                    "Patient review {ReviewId} was not deleted. It may not exist or its version may have changed",
                    request.Id);

                return false;
            }

            logger.LogInformation(
                "Deleted patient review {ReviewId}",
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
                "Failed to delete patient review");

            // LoggingBehavior records the exception.
            throw;
        }
    }
}