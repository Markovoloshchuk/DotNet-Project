using Feedback.Application.Common.Interfaces;

namespace Feedback.Application.PatientReviews.Commands.UpdatePatientReview;

public sealed record UpdatePatientReviewCommand(
    string Id,
    int ExpectedVersion,
    int Rating,
    string Comment)
    : ICommand<bool>;