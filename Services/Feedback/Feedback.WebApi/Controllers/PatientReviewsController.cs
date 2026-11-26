using Feedback.Application.Common.Models;
using Feedback.Application.PatientReviews.Commands.CreatePatientReview;
using Feedback.Application.PatientReviews.Commands.DeletePatientReview;
using Feedback.Application.PatientReviews.Commands.UpdatePatientReview;
using Feedback.Application.PatientReviews.Queries.GetPatientReviewById;
using Feedback.Application.PatientReviews.Queries.GetPatientReviewsList;
using Feedback.WebApi.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Feedback.WebApi.Controllers;

[ApiController]
[Route("api/patient-reviews")]
public sealed class PatientReviewsController(
    ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<PatientReviewDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PatientReviewDto>>> GetList(
        [FromQuery] long? patientId,
        [FromQuery] long? doctorId,
        [FromQuery] int? rating,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetPatientReviewsListQuery(
                patientId,
                doctorId,
                rating,
                page,
                pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(
        typeof(PatientReviewDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientReviewDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var review = await sender.Send(
            new GetPatientReviewByIdQuery(id),
            cancellationToken);

        if (review is null)
            return NotFound();

        return Ok(review);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePatientReviewRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePatientReviewCommand(
            request.ReviewId,
            request.PatientId,
            request.DoctorId,
            request.Rating,
            request.Comment,
            request.VerifiedVisit,
            request.Tags);

        var id = await sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            new { id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdatePatientReviewRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await sender.Send(
            new UpdatePatientReviewCommand(
                id,
                request.ExpectedVersion,
                request.Rating,
                request.Comment),
            cancellationToken);

        if (!updated)
        {
            return Conflict(new
            {
                message = "The review was changed by another request. Reload it and try again."
            });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        string id,
        [FromQuery] int expectedVersion,
        CancellationToken cancellationToken)
    {
        // Перевіряємо, чи існує відгук.
        var existing = await sender.Send(
            new GetPatientReviewByIdQuery(id),
            cancellationToken);

        if (existing is null)
            return NotFound();

        var deleted = await sender.Send(
            new DeletePatientReviewCommand(
                id,
                expectedVersion),
            cancellationToken);

        if (!deleted)
        {
            return Conflict(new
            {
                message = "The review was changed by another request. Reload it and try again."
            });
        }

        return NoContent();
    }
}