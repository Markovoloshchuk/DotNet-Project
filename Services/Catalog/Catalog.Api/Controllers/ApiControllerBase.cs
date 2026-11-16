using Catalog.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected async Task<IActionResult> ExecuteAsync(
        Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (NotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
        catch (DomainException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }
}