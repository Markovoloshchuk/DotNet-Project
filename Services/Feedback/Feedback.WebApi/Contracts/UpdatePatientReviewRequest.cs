namespace Feedback.WebApi.Contracts;

public sealed record UpdatePatientReviewRequest(
    int ExpectedVersion,
    int Rating,
    string Comment);
