using System.Text.Json;
using Platform.Aggregator.Clients;
using Platform.Aggregator.Models;

namespace Platform.Aggregator.Services;

public sealed class AppointmentAggregationService
{
    private readonly AppointmentsClient _appointmentsClient;
    private readonly CatalogClient _catalogClient;
    private readonly FeedbackClient _feedbackClient;
    private readonly ILogger<AppointmentAggregationService> _logger;

    public AppointmentAggregationService(
        AppointmentsClient appointmentsClient,
        CatalogClient catalogClient,
        FeedbackClient feedbackClient,
        ILogger<AppointmentAggregationService> logger)
    {
        _appointmentsClient = appointmentsClient;
        _catalogClient = catalogClient;
        _feedbackClient = feedbackClient;
        _logger = logger;
    }

    public async Task<AppointmentFullResponse> GetFullAsync(
        int appointmentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting aggregation for appointment {AppointmentId}",
            appointmentId);

        // Step 1: Get the required appointment.
        var appointment = await _appointmentsClient.GetByIdAsync(
            appointmentId,
            cancellationToken);

        _logger.LogInformation(
            "Appointment {AppointmentId} retrieved successfully",
            appointmentId);

        // Step 2: Fetch Catalog and Feedback concurrently.
        var catalogTask = TryGetOptionalAsync(
            "Catalog",
            _catalogClient.GetAllAsync,
            cancellationToken);

        var feedbackTask = TryGetOptionalAsync(
            "Feedback",
            ct => _feedbackClient.GetForAppointmentAsync(
                appointment,
                ct),
            cancellationToken);

        var optionalResults = await Task.WhenAll(
            catalogTask,
            feedbackTask);

        var catalogResult = optionalResults[0];
        var feedbackResult = optionalResults[1];

        var warnings = new List<string>();

        if (catalogResult.Warning is not null)
        {
            warnings.Add(catalogResult.Warning);
        }

        if (feedbackResult.Warning is not null)
        {
            warnings.Add(feedbackResult.Warning);
        }

        _logger.LogInformation(
            "Aggregation completed for appointment {AppointmentId}. " +
            "Warnings: {WarningCount}",
            appointmentId,
            warnings.Count);

        return new AppointmentFullResponse(
            Appointment: appointment,
            Catalog: catalogResult.Data,
            Feedback: feedbackResult.Data,
            Warnings: warnings);
    }

    private async Task<OptionalServiceResult> TryGetOptionalAsync(
        string serviceName,
        Func<CancellationToken, Task<JsonElement>> request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await request(cancellationToken);

            _logger.LogInformation(
                "{ServiceName} data retrieved successfully",
                serviceName);

            return new OptionalServiceResult(data, null);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
            when (exception is not OutOfMemoryException
                  && exception is not StackOverflowException)
        {
            var reason = exception switch
            {
                HttpRequestException httpException
                    when httpException.StatusCode.HasValue
                    => $"HTTP {(int)httpException.StatusCode.Value}",

                OperationCanceledException => "request timeout",

                JsonException => "invalid JSON response",

                _ => "request failed"
            };

            var warning =
                $"{serviceName} service is unavailable ({reason}).";

            _logger.LogWarning(
                exception,
                "{ServiceName} request failed. Returning partial response.",
                serviceName);

            return new OptionalServiceResult(null, warning);
        }
    }

    private sealed record OptionalServiceResult(
        JsonElement? Data,
        string? Warning);
}