using Appointments.Api.Middleware;
using Appointments.Bll;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString =
    builder.Configuration.GetConnectionString("AppointmentsDb")
    ?? throw new InvalidOperationException(
        "Connection string 'AppointmentsDb' is missing.");

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ProblemDetails
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
    };
});

// Global exception handler
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

// BLL + DAL + Unit of Work + AutoMapper
builder.Services.AddAppointmentsBll(
    connectionString,
    builder.Configuration["AutoMapper:LicenseKey"]);

var app = builder.Build();

// Exception handling
app.UseExceptionHandler();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();