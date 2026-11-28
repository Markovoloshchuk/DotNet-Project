
using Appointments.Api.Grpc;
using Appointments.Api.Middleware;
using Appointments.Bll;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Platform.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Enable HTTP/1.1 and HTTP/2 negotiation on HTTPS endpoints.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(endpointOptions =>
    {
        endpointOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

builder.AddServiceDefaults();

// Connection string.
var connectionString =
    builder.Configuration.GetConnectionString("AppointmentsDb")
    ?? throw new InvalidOperationException(
        "Connection string 'AppointmentsDb' is missing.");

// Controllers.
builder.Services.AddControllers();

// gRPC.
builder.Services.AddGrpc();

// Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ProblemDetails.
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
    };
});

// Global exception handler.
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

// BLL + DAL + Unit of Work + AutoMapper.
builder.Services.AddAppointmentsBll(
    connectionString,
    builder.Configuration["AutoMapper:LicenseKey"]);

var app = builder.Build();

// Correlation ID middleware.
app.UseMiddleware<CorrelationIdMiddleware>();

// Exception handling.
app.UseExceptionHandler();

// Swagger UI.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// HTTP REST endpoints.
app.MapControllers();

// gRPC endpoints.
app.MapGrpcService<AppointmentsGrpcService>();

app.MapDefaultEndpoints();

app.Run();
