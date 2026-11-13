using Appointments.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Appointments.Api.Middleware;

public sealed class DomainExceptionHandler(
    IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException =>
                (StatusCodes.Status404NotFound, "Resource not found"),

            BusinessConflictException =>
                (StatusCodes.Status409Conflict, "Business conflict"),

            ValidationException =>
                (StatusCodes.Status400BadRequest, "Validation error"),

            _ =>
                (StatusCodes.Status500InternalServerError, "Internal server error")
        };

        httpContext.Response.StatusCode = statusCode;

        var details = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception is DomainException
                ? exception.Message
                : "An unexpected error occurred.",
            Instance = httpContext.Request.Path
        };

        details.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = details
            });
    }
}