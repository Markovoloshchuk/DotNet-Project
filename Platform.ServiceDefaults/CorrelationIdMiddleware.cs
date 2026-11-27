using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Platform.ServiceDefaults;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.Request.Headers[HeaderName] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;

            return Task.CompletedTask;
        });

        using (LogContext.PushProperty(
            "CorrelationId",
            correlationId))
        {
            Activity.Current?.SetTag(
                "app.correlation_id",
                correlationId);

            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "HTTP request started: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            try
            {
                await _next(context);

                _logger.LogInformation(
                    "HTTP request completed: {Method} {Path}, " +
                    "StatusCode: {StatusCode}, DurationMs: {DurationMs}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.Elapsed.TotalMilliseconds);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "HTTP request failed: {Method} {Path}, " +
                    "DurationMs: {DurationMs}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.Elapsed.TotalMilliseconds);

                throw;
            }
        }
    }

    private static string GetOrCreateCorrelationId(
        HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(
                HeaderName,
                out var incomingValue)
            && Guid.TryParse(
                incomingValue.ToString(),
                out var correlationGuid))
        {
            return correlationGuid.ToString("D");
        }

        return Guid.NewGuid().ToString("D");
    }
}