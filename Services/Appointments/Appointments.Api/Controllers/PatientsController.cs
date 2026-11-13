using Appointments.Bll.DTOs.Patients;
using Appointments.Bll.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Appointments.Api.Controllers;

[ApiController]
[Route("api/patients")]
public sealed class PatientsController(
    IPatientService patientService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PatientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PatientDto>>> GetAll(
        CancellationToken ct)
    {
        var patients = await patientService.GetAllAsync(ct);

        return Ok(patients);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientDto>> GetById(
        long id,
        CancellationToken ct)
    {
        var patient = await patientService.GetByIdAsync(id, ct);

        return Ok(patient);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatientDto>> Create(
        [FromBody] CreatePatientDto input,
        CancellationToken ct)
    {
        var created = await patientService.CreateAsync(input, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.PatientId },
            created);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientDto>> Update(
        long id,
        [FromBody] UpdatePatientDto input,
        CancellationToken ct)
    {
        var updated = await patientService.UpdateAsync(id, input, ct);

        return Ok(updated);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken ct)
    {
        await patientService.DeleteAsync(id, ct);

        return NoContent();
    }
}