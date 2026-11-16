using Catalog.Bll.DTOs.Categories;
using Catalog.Bll.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ServiceCategoriesController : ApiControllerBase
{
    private readonly IServiceCategoryService _service;

    public ServiceCategoriesController(
        IServiceCategoryService service)
    {
        _service = service;
    }

    // GET: api/ServiceCategories
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ServiceCategoryDto>),
        StatusCodes.Status200OK)]
    public Task<IActionResult> GetAll(CancellationToken ct)
    {
        return ExecuteAsync(async () =>
        {
            var categories = await _service.GetAllAsync(ct);
            return Ok(categories);
        });
    }

    // GET: api/ServiceCategories/5
    [HttpGet("{id:long}")]
    [ProducesResponseType(
        typeof(ServiceCategoryDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(
        long id,
        CancellationToken ct)
    {
        return ExecuteAsync(async () =>
        {
            var category = await _service.GetByIdAsync(id, ct);
            return Ok(category);
        });
    }

    // POST: api/ServiceCategories
    [HttpPost]
    [ProducesResponseType(
        typeof(ServiceCategoryDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Create(
        [FromBody] CreateServiceCategoryDto dto,
        CancellationToken ct)
    {
        return ExecuteAsync(async () =>
        {
            var created = await _service.CreateAsync(dto, ct);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.CategoryId },
                created);
        });
    }

    // PUT: api/ServiceCategories/5
    [HttpPut("{id:long}")]
    [ProducesResponseType(
        typeof(ServiceCategoryDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Update(
        long id,
        [FromBody] UpdateServiceCategoryDto dto,
        CancellationToken ct)
    {
        return ExecuteAsync(async () =>
        {
            var updated = await _service.UpdateAsync(id, dto, ct);
            return Ok(updated);
        });
    }

    // DELETE: api/ServiceCategories/5
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Delete(
        long id,
        CancellationToken ct)
    {
        return ExecuteAsync(async () =>
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        });
    }
}