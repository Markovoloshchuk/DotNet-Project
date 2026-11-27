using Platform.Aggregator.Clients;
using Platform.Aggregator.Services;
using Platform.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ProblemDetails
builder.Services.AddProblemDetails();

// Service addresses from Aspire Service Discovery
var appointmentsAddress = GetServiceAddress(
    builder.Configuration,
    "Services:Appointments");

var catalogAddress = GetServiceAddress(
    builder.Configuration,
    "Services:Catalog");

var feedbackAddress = GetServiceAddress(
    builder.Configuration,
    "Services:Feedback");

// Typed HTTP clients
builder.Services.AddHttpClient<AppointmentsClient>(client =>
{
    client.BaseAddress = appointmentsAddress;
});

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = catalogAddress;
});

builder.Services.AddHttpClient<FeedbackClient>(client =>
{
    client.BaseAddress = feedbackAddress;
});

// Aggregation service
builder.Services.AddScoped<AppointmentAggregationService>();

var app = builder.Build();

// Correlation ID
app.UseMiddleware<CorrelationIdMiddleware>();

// Exception handling
app.UseExceptionHandler();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();

static Uri GetServiceAddress(
    IConfiguration configuration,
    string key)
{
    var value = configuration[key];

    if (string.IsNullOrWhiteSpace(value)
        || !Uri.TryCreate(value, UriKind.Absolute, out var address))
    {
        throw new InvalidOperationException(
            $"Service address '{key}' is missing or invalid.");
    }

    return address;
}