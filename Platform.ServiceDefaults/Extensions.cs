using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Platform.ServiceDefaults;
using Platform.ServiceDefaults.Observability;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    private const string MongoDbActivitySourceName = "MongoDB.Driver";

    private const string AppointmentsActivitySourceName =
        "Clinic.Appointments";

    private const string CatalogActivitySourceName =
        "Clinic.Catalog";

    private const string FeedbackActivitySourceName =
        "Clinic.Feedback";

    public static TBuilder AddServiceDefaults<TBuilder>(
        this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        ConfigureSerilog(builder);

        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        // Metrics infrastructure.
        builder.Services.AddMetrics();

        // Shared gRPC interceptors.
        builder.Services.AddSingleton<GrpcServerLoggingInterceptor>();
        builder.Services.AddSingleton<GrpcClientTracingInterceptor>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    private static void ConfigureSerilog<TBuilder>(TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddSerilog(
            (_, loggerConfiguration) =>
            {
                loggerConfiguration
                    .MinimumLevel.Is(
                        builder.Environment.IsDevelopment()
                            ? LogEventLevel.Debug
                            : LogEventLevel.Information)
                    .MinimumLevel.Override(
                        "Microsoft",
                        LogEventLevel.Warning)
                    .MinimumLevel.Override(
                        "Microsoft.Hosting.Lifetime",
                        LogEventLevel.Information)
                    .MinimumLevel.Override(
                        "Microsoft.EntityFrameworkCore",
                        LogEventLevel.Warning)
                    .MinimumLevel.Override(
                        "System.Net.Http.HttpClient",
                        LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .Enrich.WithProperty(
                        "ServiceName",
                        builder.Environment.ApplicationName)
                    .Enrich.WithProperty(
                        "Environment",
                        builder.Environment.EnvironmentName)
                    .WriteTo.Console(new CompactJsonFormatter());
            },
            writeToProviders: true);
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(
        this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.ParseStateValues = true;
        });

        var samplingRatioConfiguration =
            builder.Configuration["OpenTelemetry:SamplingRatio"];

        double samplingRatio;

        if (string.IsNullOrWhiteSpace(samplingRatioConfiguration))
        {
            samplingRatio = builder.Environment.IsDevelopment()
                ? 1.0
                : 0.1;
        }
        else if (!double.TryParse(
                     samplingRatioConfiguration,
                     NumberStyles.Float,
                     CultureInfo.InvariantCulture,
                     out samplingRatio))
        {
            throw new InvalidOperationException(
                "OpenTelemetry:SamplingRatio must be a valid number.");
        }

        if (!double.IsFinite(samplingRatio)
            || samplingRatio is < 0 or > 1)
        {
            throw new InvalidOperationException(
                "OpenTelemetry:SamplingRatio must be between 0 and 1.");
        }

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService(
                    serviceName: builder.Environment.ApplicationName))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(TelemetryNames.CatalogCacheMeter)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddSqlClientInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .SetSampler(
                        new TraceIdRatioBasedSampler(samplingRatio))
                    .AddSource(builder.Environment.ApplicationName)
                    .AddSource(MongoDbActivitySourceName)
                    .AddSource(TelemetryNames.GrpcActivitySource)

                    // Business ActivitySources.
                    .AddSource(AppointmentsActivitySourceName)
                    .AddSource(CatalogActivitySourceName)
                    .AddSource(FeedbackActivitySourceName)

                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments(
                                HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(
                                AlivenessEndpointPath);
                    })
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    });
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(
        this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter =
            !string.IsNullOrWhiteSpace(
                builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services
                .AddOpenTelemetry()
                .UseOtlpExporter();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(
        this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy(),
                ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(
        this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks(HealthEndpointPath);

            app.MapHealthChecks(
                AlivenessEndpointPath,
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("live")
                });
        }

        return app;
    }
}