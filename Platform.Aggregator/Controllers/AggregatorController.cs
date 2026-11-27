using System.Net;
using Microsoft.AspNetCore.Mvc;
using Platform.Aggregator.Models;
using Platform.Aggregator.Services;

namespace Platform.Aggregator.Controllers;

[ApiController]
[Route("api/aggregator")]
public sealed class AggregatorController : ControllerBase
{
    private readonly AppointmentAggregationService _aggregationService;

    public AggregatorController(
        AppointmentAggregationService aggregationService)
    {
        _aggregationService = aggregationService;
    }

    [HttpGet("appointments/{id:int}/full")]
    [ProducesResponseType(
        typeof(AppointmentFullResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<AppointmentFullResponse>>
        GetFullAppointment(
            int id,
            CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid appointment ID",
                Detail = "Appointment ID must be greater than zero.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var result = await _aggregationService.GetFullAsync(
                id,
                cancellationToken);

            return Ok(result);
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Appointment not found",
                Detail = $"Appointment with ID {id} was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ProblemDetails
                {
                    Title = "Appointments service unavailable",
                    Detail = "Could not retrieve the requested appointment.",
                    Status = StatusCodes.Status502BadGateway
                });
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(
                StatusCodes.Status504GatewayTimeout,
                new ProblemDetails
                {
                    Title = "Appointments service timeout",
                    Detail = "The primary service did not respond in time.",
                    Status = StatusCodes.Status504GatewayTimeout
                });
        }
    }
}