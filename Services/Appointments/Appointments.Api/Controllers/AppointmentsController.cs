using Appointments.Bll.DTOs.Appointments;
using Appointments.Bll.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Appointments.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController(
    IAppointmentService appointmentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetAll(
        CancellationToken ct)
    {
        var appointments = await appointmentService.GetAllAsync(ct);

        return Ok(appointments);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDto>> GetById(
        long id,
        CancellationToken ct)
    {
        var appointment = await appointmentService.GetByIdAsync(id, ct);

        return Ok(appointment);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDto>> Create(
        [FromBody] CreateAppointmentDto input,
        CancellationToken ct)
    {
        var created = await appointmentService.CreateAsync(input, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.AppointmentId },
            created);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDto>> Update(
        long id,
        [FromBody] UpdateAppointmentDto input,
        CancellationToken ct)
    {
        var updated = await appointmentService.UpdateAsync(id, input, ct);

        return Ok(updated);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken ct)
    {
        await appointmentService.DeleteAsync(id, ct);

        return NoContent();
    }
}