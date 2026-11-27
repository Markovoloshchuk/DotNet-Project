using Feedback.Application.Common.Mappings;
using Feedback.Application.Common.Models;
using Feedback.Application.Common.Validation;
using Feedback.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedback.Application.ConsultationNotes.Queries.GetConsultationNoteById;

public sealed record GetConsultationNoteByIdQuery(string Id)
    : IRequest<ConsultationNoteDto?>;

public sealed class GetConsultationNoteByIdQueryHandler(
    IConsultationNoteRepository repository)
    : IRequestHandler<GetConsultationNoteByIdQuery, ConsultationNoteDto?>
{
    public async Task<ConsultationNoteDto?> Handle(
        GetConsultationNoteByIdQuery request,
        CancellationToken cancellationToken)
    {
        var note = await repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        return note?.ToDto();
    }
}

public sealed class GetConsultationNoteByIdQueryValidator
    : AbstractValidator<GetConsultationNoteByIdQuery>
{
    public GetConsultationNoteByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .Must(DocumentId.IsValid);
    }
}