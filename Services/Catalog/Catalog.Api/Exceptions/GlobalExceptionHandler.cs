using System.Diagnostics;
using Catalog.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // The client has disconnected or cancelled the request.
        if (exception is OperationCanceledException &&
            httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug(
                "HTTP request was cancelled. Path: {Path}",
                httpContext.Request.Path);

            return true;
        }

        if (httpContext.Response.HasStarted)
        {
            _logger.LogError(
                exception,
                "The response has already started. " +
                "The exception cannot be handled.");

            return false;
        }

        var (statusCode, title, detail, isUnexpected) = exception switch
        {
            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                notFound.Message,
                false),

            DomainException domain => (
                StatusCodes.Status400BadRequest,
                "Business rule violation",
                domain.Message,
                false),

            ArgumentException argument => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                argument.Message,
                false),

            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Data conflict",
                "The resource was modified by another operation.",
                false),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred while processing the request.",
                true)
        };

        if (isUnexpected)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                "Request failed with status {StatusCode}. " +
                "Method: {Method}, Path: {Path}, Reason: {Reason}",
                statusCode,
                httpContext.Request.Method,
                httpContext.Request.Path,
                detail);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;

        await _problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });

        return true;
    }
}