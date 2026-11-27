using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Platform.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(
        builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseExceptionHandler();

app.MapReverseProxy(proxyPipeline =>
{
    // Structured logging for every matched YARP route.
    proxyPipeline.Use(async (context, next) =>
    {
        var proxyFeature = context.GetReverseProxyFeature();

        var routeId = proxyFeature.Route.Config.RouteId;
        var clusterId = proxyFeature.Cluster.Config.ClusterId;

        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Platform.ApiGateway.Proxy");

        var stopwatch = Stopwatch.StartNew();

        Activity.Current?.SetTag("yarp.route.id", routeId);
        Activity.Current?.SetTag("yarp.cluster.id", clusterId);

        logger.LogInformation(
            "Proxy request started: {Method} {Path}, RouteId: {RouteId}, ClusterId: {ClusterId}",
            context.Request.Method,
            context.Request.Path,
            routeId,
            clusterId);

        try
        {
            await next();

            stopwatch.Stop();

            var durationMs = stopwatch.Elapsed.TotalMilliseconds;
            var statusCode = context.Response.StatusCode;

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                Activity.Current?.SetStatus(
                    ActivityStatusCode.Error,
                    $"Proxy returned HTTP {statusCode}");

                logger.LogWarning(
                    "Proxy request completed with server error: RouteId: {RouteId}, ClusterId: {ClusterId}, StatusCode: {StatusCode}, DurationMs: {DurationMs}",
                    routeId,
                    clusterId,
                    statusCode,
                    durationMs);
            }
            else
            {
                logger.LogInformation(
                    "Proxy request completed: RouteId: {RouteId}, ClusterId: {ClusterId}, StatusCode: {StatusCode}, DurationMs: {DurationMs}",
                    routeId,
                    clusterId,
                    statusCode,
                    durationMs);
            }
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            Activity.Current?.SetStatus(
                ActivityStatusCode.Error,
                "Proxy request failed");

            logger.LogError(
                exception,
                "Proxy request failed: RouteId: {RouteId}, ClusterId: {ClusterId}, DurationMs: {DurationMs}",
                routeId,
                clusterId,
                stopwatch.Elapsed.TotalMilliseconds);

            throw;
        }
    });

    // Preserve YARP's standard proxy pipeline components.
    proxyPipeline.UseSessionAffinity();
    proxyPipeline.UseLoadBalancing();
    proxyPipeline.UsePassiveHealthChecks();
});

app.MapDefaultEndpoints();

app.Run();