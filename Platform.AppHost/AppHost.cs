var builder = DistributedApplication.CreateBuilder(args);

// Підключення до вже існуючої бази даних
var appointmentsDb = builder.AddConnectionString("AppointmentsDb");

// Реєстрація API
builder.AddProject<Projects.Appointments_Api>("appointments-api")
    .WithReference(appointmentsDb);

builder.Build().Run();  