using Catalog.Bll.DTOs.Common;
using Catalog.Bll.DTOs.MedicalServices;
using Catalog.Bll.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MedicalServicesController : ControllerBase
{
    private readonly IMedicalServiceService _service;

    public MedicalServicesController(
        IMedicalServiceService service)
    {
        _service = service;
    }

    // GET: api/MedicalServices?pageNumber=1&pageSize=10
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResultDto<MedicalServiceDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] SearchMedicalServicesDto filter,
        CancellationToken ct)
    {
        var result = await _service.SearchAsync(filter, ct);
        return Ok(result);
    }

    // GET: api/MedicalServices/5
    [HttpGet("{id:long}")]
    [ProducesResponseType(
        typeof(MedicalServiceDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        long id,
        CancellationToken ct)
    {
        var medicalService = await _service.GetByIdAsync(id, ct);
        return Ok(medicalService);
    }

    // GET: api/MedicalServices/search?name=consultation
    [HttpGet("search")]
    [ProducesResponseType(
        typeof(PagedResultDto<MedicalServiceDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] SearchMedicalServicesDto filter,
        CancellationToken ct)
    {
        var result = await _service.SearchAsync(filter, ct);
        return Ok(result);
    }

    // GET: api/MedicalServices/top-by-price?count=5
    [HttpGet("top-by-price")]
    [ProducesResponseType(
        typeof(IReadOnlyList<MedicalServiceDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopByPrice(
        [FromQuery] int count = 5,
        CancellationToken ct = default)
    {
        if (count is < 1 or > 100)
        {
            ModelState.AddModelError(
                nameof(count),
                "Count must be between 1 and 100.");

            return ValidationProblem(ModelState);
        }

        var services = await _service.GetTopByPriceAsync(count, ct);
        return Ok(services);
    }

    // POST: api/MedicalServices
    [HttpPost]
    [ProducesResponseType(
        typeof(MedicalServiceDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMedicalServiceDto dto,
        CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.ServiceId },
            created);
    }

    // PUT: api/MedicalServices/5
    [HttpPut("{id:long}")]
    [ProducesResponseType(
        typeof(MedicalServiceDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateMedicalServiceDto dto,
        CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(id, dto, ct);
        return Ok(updated);
    }

    // DELETE: api/MedicalServices/5
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}