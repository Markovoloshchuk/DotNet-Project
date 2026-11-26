using Feedback.Application.Common.Interfaces;
using Feedback.Application.Common.Models;

namespace Feedback.Application.PatientReviews.Queries.GetPatientReviewById;

public sealed record GetPatientReviewByIdQuery(string Id)
    : IQuery<PatientReviewDto?>;