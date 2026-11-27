using Feedback.Application.Common.Exceptions;
using Feedback.Application.Common.Validation;
using Feedback.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedback.Application.ConsultationNotes.Commands.UpdateConsultationNote;

public sealed record UpdateConsultationNoteCommand(
    string Id,
    int ExpectedVersion,
    string Summary,
    IReadOnlyCollection<string>? RecommendationsToAdd)
    : IRequest<bool>;

public sealed class UpdateConsultationNoteCommandHandler(
    IConsultationNoteRepository repository)
    : IRequestHandler<UpdateConsultationNoteCommand, bool>
{
    public async Task<bool> Handle(
        UpdateConsultationNoteCommand request,
        CancellationToken cancellationToken)
    {
        var note = await repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (note is null)
            throw new NotFoundException(
                "Consultation note was not found.");

        if (note.Version != request.ExpectedVersion)
            return false;

        note.UpdateSummary(request.Summary);

        if (request.RecommendationsToAdd is not null)
        {
            foreach (var recommendation in request.RecommendationsToAdd)
                note.AddRecommendation(recommendation);
        }

        return await repository.UpdateAsync(
            note,
            cancellationToken);
    }
}

public sealed class UpdateConsultationNoteCommandValidator
    : AbstractValidator<UpdateConsultationNoteCommand>
{
    public UpdateConsultationNoteCommandValidator()
    {
        RuleFor(x => x.Id)
            .Must(DocumentId.IsValid);

        RuleFor(x => x.ExpectedVersion)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Summary)
            .NotEmpty()
            .MaximumLength(5000);

        RuleForEach(x => x.RecommendationsToAdd)
            .NotEmpty()
            .MaximumLength(1000);
    }
}