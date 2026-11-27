using AutoMapper;
using Feedback.Application.Common.Models;
using Feedback.Domain.Entities;

namespace Feedback.Application.Common.Mappings;

public sealed class PatientReviewProfile : Profile
{
    public PatientReviewProfile()
    {
        CreateMap<PatientReview, PatientReviewDto>()
            .ConstructUsing(source => new PatientReviewDto(
                source.Id.ToString(),
                source.ReviewId,
                source.PatientId,
                source.DoctorId,
                source.Rating.Value,
                source.Comment,
                source.CreatedAt,
                source.UpdatedAt,
                source.ClinicResponse == null
                    ? null
                    : source.ClinicResponse.Message,
                source.HelpfulVotes,
                source.VerifiedVisit,
                source.Tags.ToArray(),
                source.Version));
    }
}