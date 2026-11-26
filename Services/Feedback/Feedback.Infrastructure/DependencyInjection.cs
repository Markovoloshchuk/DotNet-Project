using Feedback.Application.Common.Interfaces;
using Feedback.Domain.Interfaces;
using Feedback.Infrastructure.Persistence;
using Feedback.Infrastructure.Queries;
using Feedback.Infrastructure.Repositories;
using Feedback.Infrastructure.Seeding;
using Feedback.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Feedback.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // BSON mappings
        FeedbackMongoMappings.Register();

        // MongoDB settings
        services.Configure<FeedbackDatabaseSettings>(
            configuration.GetSection(
                FeedbackDatabaseSettings.SectionName));

        // MongoDB client
        services.AddSingleton<MongoClient>(serviceProvider =>
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<FeedbackDatabaseSettings>>()
                .Value;

            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton<IMongoClient>(serviceProvider =>
            serviceProvider.GetRequiredService<MongoClient>());

        // MongoDB context
        services.AddSingleton<MongoDbContext>();

        // Repositories
        services.AddScoped<
            IPatientReviewRepository,
            PatientReviewRepository>();

        services.AddScoped<
            IDoctorDiscussionRepository,
            DoctorDiscussionRepository>();

        services.AddScoped<
            IConsultationNoteRepository,
            ConsultationNoteRepository>();

        // Queries
        services.AddScoped<
            IPatientReviewQueries,
            PatientReviewQueries>();

        // Seeders
        services.AddScoped<IDataSeeder, PatientReviewSeeder>();
        services.AddScoped<IDataSeeder, DoctorDiscussionSeeder>();
        services.AddScoped<IDataSeeder, ConsultationNoteSeeder>();

        // Hosted services: indexes first, seeding second
        services.AddHostedService<IndexCreationService>();
        services.AddHostedService<DataSeedingHostedService>();

        return services;
    }
}