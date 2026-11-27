using System.Diagnostics;
using Feedback.Domain.Entities;
using Feedback.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Feedback.Application.PatientReviews.Commands.CreatePatientReview;

public sealed class CreatePatientReviewCommandHandler(
    IPatientReviewRepository repository,
    ILogger<CreatePatientReviewCommandHandler> logger)
    : IRequestHandler<CreatePatientReviewCommand, string>
{
    private static readonly ActivitySource ActivitySource =
        new("Clinic.Feedback");

    public async Task<string> Handle(
        CreatePatientReviewCommand request,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "Feedback.PatientReview.Create");

        activity?.SetTag("feedback.patient.id", request.PatientId);
        activity?.SetTag("feedback.doctor.id", request.DoctorId);

        try
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

            var reviewId = review.Id.ToString();

            activity?.SetTag("feedback.review.id", reviewId);

            logger.LogInformation(
                "Created patient review {ReviewId} for patient {PatientId} and doctor {DoctorId}",
                reviewId,
                request.PatientId,
                request.DoctorId);

            return reviewId;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Failed to create patient review");

            // LoggingBehavior records the exception.
            throw;
        }
    }
}