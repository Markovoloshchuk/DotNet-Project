using Appointments.Api.Grpc.Contracts;
using Catalog.Api.Grpc.Contracts;
using Platform.Aggregator.Clients;
using Platform.Aggregator.Services;
using Platform.ServiceDefaults;
using Platform.ServiceDefaults.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Redis distributed cache.
builder.AddRedisDistributedCache("redis");

// Controllers.
builder.Services.AddControllers();

// Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ProblemDetails.
builder.Services.AddProblemDetails();

// Service addresses from Aspire Service Discovery.
var appointmentsAddress = GetServiceAddress(
    builder.Configuration,
    "Services:Appointments");

var catalogAddress = GetServiceAddress(
    builder.Configuration,
    "Services:Catalog");

var feedbackAddress = GetServiceAddress(
    builder.Configuration,
    "Services:Feedback");

// Appointments gRPC client with tracing interceptor.
builder.Services
    .AddGrpcClient<AppointmentsGrpc.AppointmentsGrpcClient>(
        options =>
        {
            options.Address = appointmentsAddress;
        })
    .AddInterceptor<GrpcClientTracingInterceptor>();

// Catalog gRPC client with tracing interceptor.
builder.Services
    .AddGrpcClient<CatalogGrpc.CatalogGrpcClient>(
        options =>
        {
            options.Address = catalogAddress;
        })
    .AddInterceptor<GrpcClientTracingInterceptor>();

// Feedback remains an HTTP client.
builder.Services.AddHttpClient<FeedbackClient>(client =>
{
    client.BaseAddress = feedbackAddress;
});

// Aggregation service.
builder.Services.AddScoped<AppointmentAggregationService>();

var app = builder.Build();

// Correlation ID.
app.UseMiddleware<CorrelationIdMiddleware>();

// Exception handling.
app.UseExceptionHandler();

// Swagger.
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