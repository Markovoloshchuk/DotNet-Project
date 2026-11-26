using Feedback.Application.Common.Exceptions;
using Feedback.Application.Common.Mappings;
using Feedback.Application.Common.Models;
using Feedback.Domain.Interfaces;
using FluentValidation;
using MediatR;
using MongoDB.Bson;

namespace Feedback.Application.DoctorDiscussions.Queries.GetDoctorDiscussionById;

public sealed record GetDoctorDiscussionByIdQuery(string Id)
    : IRequest<DoctorDiscussionDto?>;

public sealed class GetDoctorDiscussionByIdQueryHandler(
    IDoctorDiscussionRepository repository)
    : IRequestHandler<GetDoctorDiscussionByIdQuery, DoctorDiscussionDto?>
{
    public async Task<DoctorDiscussionDto?> Handle(
        GetDoctorDiscussionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var discussion = await repository.GetByIdAsync(
            ObjectId.Parse(request.Id),
            cancellationToken);

        return discussion?.ToDto();
    }
}

public sealed class GetDoctorDiscussionByIdQueryValidator
    : AbstractValidator<GetDoctorDiscussionByIdQuery>
{
    public GetDoctorDiscussionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(id => ObjectId.TryParse(id, out _));
    }
}