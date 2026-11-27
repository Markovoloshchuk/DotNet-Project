using Feedback.Application.Common.Exceptions;
using Feedback.Application.Common.Validation;
using Feedback.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedback.Application.DoctorDiscussions.Commands.UpdateDoctorDiscussion;

public sealed record UpdateDoctorDiscussionCommand(
    string Id,
    int ExpectedVersion,
    string Title,
    string Content,
    IReadOnlyCollection<string>? TagsToAdd)
    : IRequest<bool>;

public sealed class UpdateDoctorDiscussionCommandHandler(
    IDoctorDiscussionRepository repository)
    : IRequestHandler<UpdateDoctorDiscussionCommand, bool>
{
    public async Task<bool> Handle(
        UpdateDoctorDiscussionCommand request,
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

        discussion.UpdateContent(
            request.Title,
            request.Content);

        if (request.TagsToAdd is not null)
        {
            foreach (var tag in request.TagsToAdd)
                discussion.AddTag(tag);
        }

        return await repository.UpdateAsync(
            discussion,
            cancellationToken);
    }
}

public sealed class UpdateDoctorDiscussionCommandValidator
    : AbstractValidator<UpdateDoctorDiscussionCommand>
{
    public UpdateDoctorDiscussionCommandValidator()
    {
        RuleFor(x => x.Id)
            .Must(DocumentId.IsValid);

        RuleFor(x => x.ExpectedVersion)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(10000);

        RuleForEach(x => x.TagsToAdd)
            .NotEmpty()
            .MaximumLength(50);
    }
}