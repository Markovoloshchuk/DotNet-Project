using Feedback.Application.Common.Exceptions;
using Feedback.Application.Common.Validation;
using Feedback.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedback.Application.DoctorDiscussions.Commands.DeleteDoctorDiscussion;

public sealed record DeleteDoctorDiscussionCommand(
    string Id,
    int ExpectedVersion)
    : IRequest<bool>;

public sealed class DeleteDoctorDiscussionCommandHandler(
    IDoctorDiscussionRepository repository)
    : IRequestHandler<DeleteDoctorDiscussionCommand, bool>
{
    public async Task<bool> Handle(
        DeleteDoctorDiscussionCommand request,
        CancellationToken cancellationToken)
    {
        var discussion = await repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (discussion is null)
            throw new NotFoundException(
                "Doctor discussion was not found.");

        if (discussion.Version != request.ExpectedVersion)
            return false;

        return await repository.DeleteAsync(
            request.Id,
            request.ExpectedVersion,
            cancellationToken);
    }
}

public sealed class DeleteDoctorDiscussionCommandValidator
    : AbstractValidator<DeleteDoctorDiscussionCommand>
{
    public DeleteDoctorDiscussionCommandValidator()
    {
        RuleFor(x => x.Id)
            .Must(DocumentId.IsValid);

        RuleFor(x => x.ExpectedVersion)
            .GreaterThanOrEqualTo(0);
    }
}