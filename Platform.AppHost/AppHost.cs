var builder = DistributedApplication.CreateBuilder(args);

// SQL Server
var sqlPassword = builder.AddParameter(
    "sql-password",
    secret: true);

var sql = builder.AddSqlServer(
        "sql",
        password: sqlPassword)
    .WithDataVolume();

var appointmentsDb = sql.AddDatabase("AppointmentsDb");
var catalogDb = sql.AddDatabase("CatalogDb");

// MongoDB
var mongodb = builder.AddMongoDB("mongodb")
    .WithDataVolume();

var feedbackDb = mongodb.AddDatabase("FeedbackDb");

// Appointments API
var appointmentsApi = builder
    .AddProject<Projects.Appointments_Api>("appointments-api")
    .WithReference(appointmentsDb)
    .WaitFor(appointmentsDb);

// Catalog API
var catalogApi = builder
    .AddProject<Projects.Catalog_Api>("catalog-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

// Feedback API
var feedbackApi = builder
    .AddProject<Projects.Feedback_WebApi>("feedback-api")
    .WithReference(feedbackDb)
    .WaitFor(feedbackDb);

// Aggregator
var aggregator = builder
    .AddProject<Projects.Platform_Aggregator>("aggregator")
    .WithReference(appointmentsApi)
    .WithReference(catalogApi)
    .WithReference(feedbackApi)
    .WaitFor(appointmentsApi)
    .WaitFor(catalogApi)
    .WaitFor(feedbackApi);

// API Gateway
builder
    .AddProject<Projects.Platform_ApiGateway>("gateway")
    .WithReference(appointmentsApi)
    .WithReference(catalogApi)
    .WithReference(feedbackApi)
    .WithReference(aggregator)
    .WaitFor(aggregator)
    .WithHttpEndpoint(port: 5000, name: "http")
    .WithExternalHttpEndpoints();

builder.Build().Run();