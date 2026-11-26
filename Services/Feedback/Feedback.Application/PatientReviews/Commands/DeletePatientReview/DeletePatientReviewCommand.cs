using Feedback.Application.Common.Interfaces;

namespace Feedback.Application.PatientReviews.Commands.DeletePatientReview;

public sealed record DeletePatientReviewCommand(
    string Id,
    int ExpectedVersion)
    : ICommand<bool>;