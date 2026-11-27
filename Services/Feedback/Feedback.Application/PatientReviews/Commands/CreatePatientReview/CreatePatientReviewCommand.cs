using Feedback.Application.Common.Interfaces;

namespace Feedback.Application.PatientReviews.Commands.CreatePatientReview;

public sealed record CreatePatientReviewCommand(
    long ReviewId,
    long PatientId,
    long DoctorId,
    int Rating,
    string Comment,
    bool? VerifiedVisit = null,
    IReadOnlyCollection<string>? Tags = null)
    : ICommand<string>;