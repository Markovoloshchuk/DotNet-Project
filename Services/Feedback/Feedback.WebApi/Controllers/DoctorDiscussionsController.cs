using Feedback.Application.Common.Models;
using Feedback.Application.DoctorDiscussions.Commands.AddDiscussionComment;
using Feedback.Application.DoctorDiscussions.Commands.CreateDoctorDiscussion;
using Feedback.Application.DoctorDiscussions.Commands.DeleteDoctorDiscussion;
using Feedback.Application.DoctorDiscussions.Commands.UpdateDoctorDiscussion;
using Feedback.Application.DoctorDiscussions.Queries.GetDoctorDiscussionById;
using Feedback.WebApi.Contracts;
using Feedback.WebApi.Http;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Feedback.WebApi.Controllers;

[ApiController]
[Route("api/feedback/doctor-discussions")]
public sealed class DoctorDiscussionsController(
    ISender sender) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(
        typeof(DoctorDiscussionDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDiscussionDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var discussion = await sender.Send(
            new GetDoctorDiscussionByIdQuery(id),
            cancellationToken);

        if (discussion is null)
            return NotFound();

        VersionTag.Write(Response, discussion.Version);

        return Ok(discussion);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDoctorDiscussionRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(
            new CreateDoctorDiscussionCommand(
                request.DiscussionId,
                request.Title,
                request.AuthorDoctorId,
                request.Content,
                request.Tags),
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
        [FromBody] UpdateDoctorDiscussionRequest request,
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
            new UpdateDoctorDiscussionCommand(
                id,
                expectedVersion,
                request.Title,
                request.Content,
                request.TagsToAdd),
            cancellationToken);

        if (!updated)
        {
            return Conflict(new
            {
                message = "The discussion was changed by another request. Reload it and try again."
            });
        }

        return NoContent();
    }

    [HttpPost("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]

    public async Task<IActionResult> AddComment(
        string id,
        [FromBody] AddDiscussionCommentRequest request,
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
            new AddDiscussionCommentCommand(
                id,
                expectedVersion,
                request.CommentId,
                request.DoctorRefId,
                request.Text),
            cancellationToken);

        if (!updated)
        {
            return Conflict(new
            {
                message = "The discussion was changed by another request. Reload it and try again."
            });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    
    public async Task<IActionResult> Delete(
        string id,
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

        var deleted = await sender.Send(
            new DeleteDoctorDiscussionCommand(
                id,
                expectedVersion),
            cancellationToken);

        if (!deleted)
        {
            return Conflict(new
            {
                message = "The discussion was changed by another request. Reload it and try again."
            });
        }

        return NoContent();
    }
}