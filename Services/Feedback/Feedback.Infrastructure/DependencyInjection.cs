using Feedback.Domain.Interfaces;
using Feedback.Application.Common.Interfaces;
using Feedback.Infrastructure.Persistence;
using Feedback.Infrastructure.Queries;
using Feedback.Infrastructure.Repositories;
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
        FeedbackMongoMappings.Register();

        services.Configure<FeedbackDatabaseSettings>(
            configuration.GetSection(
                FeedbackDatabaseSettings.SectionName));

        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<FeedbackDatabaseSettings>>()
                .Value;

            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton<IMongoDatabase>(serviceProvider =>
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<FeedbackDatabaseSettings>>()
                .Value;

            var client = serviceProvider
                .GetRequiredService<IMongoClient>();

            return client.GetDatabase(settings.DatabaseName);
        });

        services.AddScoped<
            IPatientReviewRepository,
            PatientReviewRepository>();

        services.AddScoped<
            IDoctorDiscussionRepository,
            DoctorDiscussionRepository>();

        services.AddScoped<
            IConsultationNoteRepository,
            ConsultationNoteRepository>();

        services.AddScoped<
            IPatientReviewQueries,
            PatientReviewQueries>();

        return services;
    }
}