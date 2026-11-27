using Feedback.Application.Common.Exceptions;
using Feedback.Application.Common.Validation;
using Feedback.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedback.Application.DoctorDiscussions.Commands.AddDiscussionComment;

public sealed record AddDiscussionCommentCommand(
    string Id,
    int ExpectedVersion,
    long CommentId,
    long DoctorRefId,
    string Text)
    : IRequest<bool>;

public sealed class AddDiscussionCommentCommandHandler(
    IDoctorDiscussionRepository repository)
    : IRequestHandler<AddDiscussionCommentCommand, bool>
{
    public async Task<bool> Handle(
        AddDiscussionCommentCommand request,
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

        discussion.AddComment(
            request.CommentId,
            request.DoctorRefId,
            request.Text);

        return await repository.UpdateAsync(
            discussion,
            cancellationToken);
    }
}

public sealed class AddDiscussionCommentCommandValidator
    : AbstractValidator<AddDiscussionCommentCommand>
{
    public AddDiscussionCommentCommandValidator()
    {
        RuleFor(x => x.Id)
            .Must(DocumentId.IsValid);

        RuleFor(x => x.ExpectedVersion)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CommentId)
            .GreaterThan(0);

        RuleFor(x => x.DoctorRefId)
            .GreaterThan(0);

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(2000);
    }
}