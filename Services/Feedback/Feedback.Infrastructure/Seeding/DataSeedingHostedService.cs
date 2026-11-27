using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Feedback.Infrastructure.Seeding;

public sealed class DataSeedingHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<DataSeedingHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var seeders = scope.ServiceProvider
            .GetServices<IDataSeeder>();

        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync(ct);
        }

        logger.LogInformation(
            "MongoDB data seeding completed.");
    }

    public Task StopAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}