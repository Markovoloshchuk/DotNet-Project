namespace Feedback.Application.Common.Models;

public sealed record PatientReviewDto(
    string Id,
    long ReviewId,
    long PatientId,
    long DoctorId,
    int Rating,
    string Comment,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? ClinicResponse,
    int HelpfulVotes,
    bool? VerifiedVisit,
    IReadOnlyCollection<string> Tags,
    int Version);