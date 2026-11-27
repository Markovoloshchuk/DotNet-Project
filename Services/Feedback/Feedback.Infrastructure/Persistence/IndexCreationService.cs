using Feedback.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Feedback.Infrastructure.Persistence;

public sealed class IndexCreationService(
    MongoDbContext context,
    ILogger<IndexCreationService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        await CreatePatientReviewIndexes(ct);
        await CreateDoctorDiscussionIndexes(ct);
        await CreateConsultationNoteIndexes(ct);

        logger.LogInformation(
            "MongoDB indexes successfully created.");
    }

    private async Task CreatePatientReviewIndexes(
        CancellationToken ct)
    {
        var keys = Builders<PatientReview>.IndexKeys;

        await context.PatientReviews.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<PatientReview>(
                    keys.Ascending("doctorId")
                        .Descending("rating"),
                    new CreateIndexOptions
                    {
                        Name = "doctorId_1_rating_-1"
                    }),

                new CreateIndexModel<PatientReview>(
                    keys.Ascending("reviewId"),
                    new CreateIndexOptions
                    {
                        Unique = true,
                        Name = "reviewId_1"
                    }),

                new CreateIndexModel<PatientReview>(
                    keys.Ascending("patientId")
                        .Ascending("doctorId"),
                    new CreateIndexOptions
                    {
                        Unique = true,
                        Name = "patientId_1_doctorId_1"
                    }),

                new CreateIndexModel<PatientReview>(
                    keys.Text("comment"),
                    new CreateIndexOptions
                    {
                        Name = "comment_text"
                    })
            },
            cancellationToken: ct);
    }

    private async Task CreateDoctorDiscussionIndexes(
        CancellationToken ct)
    {
        var keys = Builders<DoctorDiscussion>.IndexKeys;

        await context.DoctorDiscussions.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<DoctorDiscussion>(
                    keys.Ascending("tags"),
                    new CreateIndexOptions
                    {
                        Name = "tags_1"
                    }),

                new CreateIndexModel<DoctorDiscussion>(
                    keys.Ascending("discussionId"),
                    new CreateIndexOptions
                    {
                        Unique = true,
                        Name = "discussionId_1"
                    }),

                new CreateIndexModel<DoctorDiscussion>(
                    keys.Combine(
                        keys.Text("title"),
                        keys.Text("content")),
                    new CreateIndexOptions
                    {
                        Name = "title_text_content_text"
                    })
            },
            cancellationToken: ct);
    }

    private async Task CreateConsultationNoteIndexes(
        CancellationToken ct)
    {
        var keys = Builders<ConsultationNote>.IndexKeys;

        await context.ConsultationNotes.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<ConsultationNote>(
                    keys.Ascending("appointmentId"),
                    new CreateIndexOptions
                    {
                        Unique = true,
                        Name = "appointmentId_1"
                    }),

                new CreateIndexModel<ConsultationNote>(
                    keys.Ascending("noteId"),
                    new CreateIndexOptions
                    {
                        Unique = true,
                        Name = "noteId_1"
                    })
            },
            cancellationToken: ct);
    }

    public Task StopAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}