using Feedback.Application.Common.Exceptions;
using Feedback.Domain.Interfaces;
using FluentValidation;
using MediatR;
using MongoDB.Bson;

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
        var id = ObjectId.Parse(request.Id);

        var discussion = await repository.GetByIdAsync(
            id,
            cancellationToken);

        if (discussion is null)
            throw new NotFoundException(
                "Doctor discussion was not found.");

        if (discussion.Version != request.ExpectedVersion)
            return false;

        return await repository.DeleteAsync(
            id,
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
            .NotEmpty()
            .Must(id => ObjectId.TryParse(id, out _));

        RuleFor(x => x.ExpectedVersion)
            .GreaterThanOrEqualTo(0);
    }
}