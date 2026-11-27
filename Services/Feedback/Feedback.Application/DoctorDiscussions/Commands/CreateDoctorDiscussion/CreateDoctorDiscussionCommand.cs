using Feedback.Domain.Entities;
using Feedback.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedback.Application.DoctorDiscussions.Commands.CreateDoctorDiscussion;

public sealed record CreateDoctorDiscussionCommand(
    long DiscussionId,
    string Title,
    long AuthorDoctorId,
    string Content,
    IReadOnlyCollection<string>? Tags)
    : IRequest<string>;

public sealed class CreateDoctorDiscussionCommandHandler(
    IDoctorDiscussionRepository repository)
    : IRequestHandler<CreateDoctorDiscussionCommand, string>
{
    public async Task<string> Handle(
        CreateDoctorDiscussionCommand request,
        CancellationToken cancellationToken)
    {
        var discussion = new DoctorDiscussion(
            request.DiscussionId,
            request.Title,
            request.AuthorDoctorId,
            request.Content,
            request.Tags);

        await repository.AddAsync(discussion, cancellationToken);

        return discussion.Id.ToString();
    }
}

public sealed class CreateDoctorDiscussionCommandValidator
    : AbstractValidator<CreateDoctorDiscussionCommand>
{
    public CreateDoctorDiscussionCommandValidator()
    {
        RuleFor(x => x.DiscussionId).GreaterThan(0);
        RuleFor(x => x.AuthorDoctorId).GreaterThan(0);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(10000);

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .MaximumLength(50);
    }
}