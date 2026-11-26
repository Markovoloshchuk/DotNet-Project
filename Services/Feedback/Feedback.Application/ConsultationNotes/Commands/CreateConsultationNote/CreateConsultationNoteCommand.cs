using Feedback.Domain.Entities;
using Feedback.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedback.Application.ConsultationNotes.Commands.CreateConsultationNote;

public sealed record CreateConsultationNoteCommand(
    long NoteId,
    long AppointmentId,
    string Summary,
    IReadOnlyCollection<string>? Recommendations)
    : IRequest<string>;

public sealed class CreateConsultationNoteCommandHandler(
    IConsultationNoteRepository repository)
    : IRequestHandler<CreateConsultationNoteCommand, string>
{
    public async Task<string> Handle(
        CreateConsultationNoteCommand request,
        CancellationToken cancellationToken)
    {
        var note = new ConsultationNote(
            request.NoteId,
            request.AppointmentId,
            request.Summary,
            request.Recommendations);

        await repository.AddAsync(note, cancellationToken);

        return note.Id.ToString();
    }
}

public sealed class CreateConsultationNoteCommandValidator
    : AbstractValidator<CreateConsultationNoteCommand>
{
    public CreateConsultationNoteCommandValidator(
        IConsultationNoteRepository repository)
    {
        RuleFor(x => x.NoteId).GreaterThan(0);

        RuleFor(x => x.AppointmentId)
            .GreaterThan(0)
            .MustAsync(async (appointmentId, cancellationToken) =>
                await repository.GetByAppointmentIdAsync(
                    appointmentId,
                    cancellationToken) is null)
            .WithMessage(
                "A consultation note already exists for this appointment.");

        RuleFor(x => x.Summary)
            .NotEmpty()
            .MaximumLength(5000);

        RuleForEach(x => x.Recommendations)
            .NotEmpty()
            .MaximumLength(1000);
    }
}