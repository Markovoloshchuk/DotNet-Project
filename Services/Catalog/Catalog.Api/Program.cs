using Catalog.Api.Data;
using Catalog.Api.Exceptions;
using Catalog.Api.Filters;
using Catalog.Api.Validators;
using Catalog.Bll;
using Catalog.Dal;
using Catalog.Dal.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Platform.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Database connection
var connectionString =
    builder.Configuration.GetConnectionString("CatalogDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'CatalogDb' was not found.");
}

// Entity Framework Core + SQL command logging
builder.Services.AddDbContext<CatalogDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(connectionString);

        var efLogger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Catalog.EFCore.Database.Command");

        options.LogTo(
            message => efLogger.LogInformation(
                "{EfCoreSqlCommand}",
                message.TrimEnd()),
            new[]
            {
                DbLoggerCategory.Database.Command.Name
            },
            LogLevel.Information);
    });

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

// Apply migrations before programmatic seeding.
// Both operations run only in Development.
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<CatalogDbContext>();

    // Apply all pending EF Core migrations.
    await dbContext.Database.MigrateAsync();

    // Seed additional development data.
    var seeder = scope.ServiceProvider
        .GetRequiredService<CatalogDbSeeder>();

    await seeder.SeedAsync();
}

// Correlation ID middleware
app.UseMiddleware<CorrelationIdMiddleware>();

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
app.MapDefaultEndpoints();

app.Run();