using System.Text.Json;

namespace Platform.Aggregator.Models;

public sealed record AppointmentFullResponse(
    JsonElement Appointment,
    JsonElement? Catalog,
    JsonElement? Feedback,
    IReadOnlyList<string> Warnings);