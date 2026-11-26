using Feedback.Application;
using Feedback.Application.Common.Mappings;
using Feedback.Infrastructure;
using Feedback.WebApi.ExceptionHandling;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application: MediatR, Behaviors, FluentValidation
builder.Services.AddApplication(builder.Configuration);

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.LicenseKey =
        builder.Configuration["LuckyPenny:LicenseKey"];

    cfg.AddMaps(typeof(PatientReviewProfile).Assembly);
});

// Infrastructure: MongoDB, repositories, seeders, indexes
builder.Services.AddInfrastructure(builder.Configuration);

// MongoDB Health Check
builder.Services
    .AddHealthChecks()
    .AddMongoDb();

// Global exception handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health endpoint
app.MapHealthChecks("/health");

app.Run();