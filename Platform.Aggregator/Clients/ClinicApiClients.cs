using System.Net.Http.Json;
using System.Text.Json;

namespace Platform.Aggregator.Clients;

public abstract class JsonApiClient
{
    protected readonly HttpClient HttpClient;

    protected JsonApiClient(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected async Task<JsonElement> GetJsonAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        using var response = await HttpClient.GetAsync(
            relativePath,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonElement>(
                   cancellationToken: cancellationToken);
    }
}

public sealed class AppointmentsClient : JsonApiClient
{
    public AppointmentsClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public Task<JsonElement> GetByIdAsync(
        int appointmentId,
        CancellationToken cancellationToken)
    {
        return GetJsonAsync(
            $"api/appointments/{appointmentId}",
            cancellationToken);
    }
}

public sealed class CatalogClient : JsonApiClient
{
    public CatalogClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public Task<JsonElement> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return GetJsonAsync(
            "api/catalog",
            cancellationToken);
    }
}

public sealed class FeedbackClient : JsonApiClient
{
    public FeedbackClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public Task<JsonElement> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return GetJsonAsync(
            "api/feedback",
            cancellationToken);
    }
}