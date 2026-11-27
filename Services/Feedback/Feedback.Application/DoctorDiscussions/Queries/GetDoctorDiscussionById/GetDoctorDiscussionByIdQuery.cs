using Feedback.Application.Common.Exceptions;
using Feedback.Application.Common.Mappings;
using Feedback.Application.Common.Models;
using Feedback.Application.Common.Validation;
using Feedback.Domain.Interfaces;
using FluentValidation;
using MediatR;

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
            request.Id,
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
            .Must(DocumentId.IsValid);
    }
}