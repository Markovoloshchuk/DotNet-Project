using Feedback.Application.Common.Exceptions;
using Feedback.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Feedback.WebApi.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            ValidationException validationException =>
            (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                "One or more validation errors occurred."
            ),

            NotFoundException =>
            (
                StatusCodes.Status404NotFound,
                "Resource not found",
                exception.Message
            ),

            DomainException =>
            (
                StatusCodes.Status400BadRequest,
                "Domain rule violation",
                exception.Message
            ),

            _ =>
            (
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred."
            )
        };

        if (statusCode >= 500)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Path}",
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Request failed with status {StatusCode}",
                statusCode);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        if (exception is ValidationException validation)
        {
            problemDetails.Extensions["errors"] =
                validation.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error => error.ErrorMessage)
                            .ToArray());
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}