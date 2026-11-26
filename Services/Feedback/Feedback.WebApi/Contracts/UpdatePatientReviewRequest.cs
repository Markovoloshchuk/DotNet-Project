namespace Feedback.WebApi.Contracts;

public sealed record UpdatePatientReviewRequest(
    int Rating,
    string Comment);