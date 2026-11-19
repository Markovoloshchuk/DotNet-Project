using Catalog.Api.Data;
using Catalog.Api.Exceptions;
using Catalog.Api.Filters;
using Catalog.Api.Validators;
using Catalog.Bll;
using Catalog.Dal;
using Catalog.Dal.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((_, _, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Information()
        .MinimumLevel.Override(
            "Microsoft",
            LogEventLevel.Warning)
        .MinimumLevel.Override(
            "Microsoft.Hosting.Lifetime",
            LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Catalog.Api")
        .WriteTo.Console(
            outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} " +
                "{Level:u3}] {SourceContext} {Message:lj}" +
                "{NewLine}{Exception}");
});

// Database connection
var connectionString =
    builder.Configuration.GetConnectionString("CatalogDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'CatalogDb' was not found.");
}

// Entity Framework Core
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(connectionString));

// Data Access Layer
builder.Services.AddCatalogDal();

// Business Logic Layer
builder.Services.AddCatalogBll(
    builder.Configuration["AutoMapper:LicenseKey"]);

// Database seeder
builder.Services.AddScoped<CatalogDbSeeder>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<
    CreateMedicalServiceDtoValidator>();

builder.Services.AddScoped<FluentValidationActionFilter>();

// Controllers
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<FluentValidationActionFilter>();
});

// Global exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed development database
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();

    var seeder = scope.ServiceProvider
        .GetRequiredService<CatalogDbSeeder>();

    await seeder.SeedAsync();
}

// Request logging
app.UseSerilogRequestLogging();

// Global exception handling middleware
app.UseExceptionHandler();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTP pipeline
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}