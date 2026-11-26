using Feedback.Domain.Entities;
using Feedback.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Feedback.Infrastructure.Persistence;

public sealed class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public IMongoCollection<PatientReview> PatientReviews { get; }

    public IMongoCollection<DoctorDiscussion> DoctorDiscussions { get; }

    public IMongoCollection<ConsultationNote> ConsultationNotes { get; }

    public MongoDbContext(
        IMongoClient client,
        IOptions<FeedbackDatabaseSettings> options)
    {
        var settings = options.Value;

        Database = client.GetDatabase(settings.DatabaseName);

        PatientReviews = Database.GetCollection<PatientReview>(
            settings.PatientReviewsCollection);

        DoctorDiscussions = Database.GetCollection<DoctorDiscussion>(
            settings.DoctorDiscussionsCollection);

        ConsultationNotes = Database.GetCollection<ConsultationNote>(
            settings.ConsultationNotesCollection);
    }
}