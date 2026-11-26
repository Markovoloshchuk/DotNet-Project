namespace Feedback.WebApi.Contracts;

public sealed record CreatePatientReviewRequest(
    long ReviewId,
    long PatientId,
    long DoctorId,
    int Rating,
    string Comment,
    bool? VerifiedVisit = null,
    IReadOnlyCollection<string>? Tags = null);