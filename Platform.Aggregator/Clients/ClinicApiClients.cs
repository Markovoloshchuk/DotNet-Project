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

public sealed class FeedbackClient : JsonApiClient
{
    public FeedbackClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public Task<JsonElement> GetForAppointmentAsync(
        long patientId,
        long doctorId,
        CancellationToken cancellationToken)
    {
        var relativePath =
            $"api/feedback/patient-reviews?patientId={patientId}" +
            $"&doctorId={doctorId}&page=1&pageSize=100";

        return GetJsonAsync(
            relativePath,
            cancellationToken);
    }
}