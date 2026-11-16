using Catalog.Dal;
using Catalog.Dal.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("CatalogDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'CatalogDb' was not found.");
}

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddCatalogDal();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();