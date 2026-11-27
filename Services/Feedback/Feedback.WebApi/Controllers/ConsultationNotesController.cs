using Feedback.Application.Common.Models;
using Feedback.Application.ConsultationNotes.Commands.CreateConsultationNote;
using Feedback.Application.ConsultationNotes.Commands.UpdateConsultationNote;
using Feedback.Application.ConsultationNotes.Queries.GetConsultationNoteByAppointmentId;
using Feedback.Application.ConsultationNotes.Queries.GetConsultationNoteById;
using Feedback.WebApi.Contracts;
using Feedback.WebApi.Http;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Feedback.WebApi.Controllers;

[ApiController]
[Route("api/consultation-notes")]
public sealed class ConsultationNotesController(
    ISender sender) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(
        typeof(ConsultationNoteDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultationNoteDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var note = await sender.Send(
            new GetConsultationNoteByIdQuery(id),
            cancellationToken);

        if (note is null)
            return NotFound();

        VersionTag.Write(Response, note.Version);

        return Ok(note);
    }

    [HttpGet("by-appointment/{appointmentId:long}")]
    [ProducesResponseType(
        typeof(ConsultationNoteDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultationNoteDto>> GetByAppointmentId(
        long appointmentId,
        CancellationToken cancellationToken)
    {
        var note = await sender.Send(
            new GetConsultationNoteByAppointmentIdQuery(appointmentId),
            cancellationToken);

        if (note is null)
            return NotFound();

        VersionTag.Write(Response, note.Version);

        return Ok(note);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateConsultationNoteRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(
            new CreateConsultationNoteCommand(
                request.NoteId,
                request.AppointmentId,
                request.Summary,
                request.Recommendations),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            new { id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateConsultationNoteRequest request,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken cancellationToken)
    {
        if (!VersionTag.TryRead(ifMatch, out var expectedVersion))
        {
            return BadRequest(new
            {
                message = "A valid If-Match header is required."
            });
        }

        var updated = await sender.Send(
            new UpdateConsultationNoteCommand(
                id,
                expectedVersion,
                request.Summary,
                request.RecommendationsToAdd),
            cancellationToken);

        if (!updated)
        {
            return Conflict(new
            {
                message = "The note was changed by another request. Reload it and try again."
            });
        }

        return NoContent();
    }
}