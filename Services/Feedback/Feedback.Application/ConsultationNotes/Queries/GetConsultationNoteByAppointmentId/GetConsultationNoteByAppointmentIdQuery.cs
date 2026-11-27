using Feedback.Application.Common.Mappings;
using Feedback.Application.Common.Models;
using Feedback.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedback.Application.ConsultationNotes.Queries.GetConsultationNoteByAppointmentId;

public sealed record GetConsultationNoteByAppointmentIdQuery(
    long AppointmentId)
    : IRequest<ConsultationNoteDto?>;

public sealed class GetConsultationNoteByAppointmentIdQueryHandler(
    IConsultationNoteRepository repository)
    : IRequestHandler<
        GetConsultationNoteByAppointmentIdQuery,
        ConsultationNoteDto?>
{
    public async Task<ConsultationNoteDto?> Handle(
        GetConsultationNoteByAppointmentIdQuery request,
        CancellationToken cancellationToken)
    {
        var note = await repository.GetByAppointmentIdAsync(
            request.AppointmentId,
            cancellationToken);

        return note?.ToDto();
    }
}

public sealed class GetConsultationNoteByAppointmentIdQueryValidator
    : AbstractValidator<GetConsultationNoteByAppointmentIdQuery>
{
    public GetConsultationNoteByAppointmentIdQueryValidator()
    {
        RuleFor(x => x.AppointmentId).GreaterThan(0);
    }
}