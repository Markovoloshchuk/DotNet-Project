using Feedback.Domain.Entities;
using Feedback.Domain.ValueObjects;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Feedback.Infrastructure.Persistence;

public static class FeedbackMongoMappings
{
    private static readonly object Sync = new();
    private static bool _registered;

    public static void Register()
    {
        lock (Sync)
        {
            if (_registered)
                return;

            var conventions = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true)
            };

            ConventionRegistry.Register(
                "FeedbackDomainConventions",
                conventions,
                type => type.Namespace?.StartsWith(
                    "Feedback.Domain",
                    StringComparison.Ordinal) == true);

            BsonSerializer.RegisterSerializer(
                typeof(Rating),
                new RatingSerializer());

            RegisterClinicResponse();
            RegisterDiscussionComment();
            RegisterPatientReview();
            RegisterDoctorDiscussion();
            RegisterConsultationNote();

            _registered = true;
        }
    }

    private static void RegisterClinicResponse()
    {
        if (BsonClassMap.IsClassMapRegistered(
                typeof(ClinicResponse)))
            return;

        BsonClassMap.RegisterClassMap<ClinicResponse>(classMap =>
        {
            classMap.AutoMap();

            classMap.MapProperty(x => x.RespondedAt);
            classMap.MapProperty(x => x.Message);

            classMap.MapCreator(
                x => new ClinicResponse(
                    x.RespondedAt,
                    x.Message));

            classMap.SetIgnoreExtraElements(true);
        });
    }

    private static void RegisterDiscussionComment()
    {
        if (BsonClassMap.IsClassMapRegistered(
                typeof(DiscussionComment)))
            return;

        BsonClassMap.RegisterClassMap<DiscussionComment>(classMap =>
        {
            classMap.AutoMap();
            classMap.SetIgnoreExtraElements(true);
        });
    }

    private static void RegisterPatientReview()
    {
        if (BsonClassMap.IsClassMapRegistered(
                typeof(PatientReview)))
            return;

        BsonClassMap.RegisterClassMap<PatientReview>(classMap =>
        {
            classMap.AutoMap();
            classMap.SetIgnoreExtraElements(true);

            classMap.MapField("_tags")
                .SetElementName("tags");
        });
    }

    private static void RegisterDoctorDiscussion()
    {
        if (BsonClassMap.IsClassMapRegistered(
                typeof(DoctorDiscussion)))
            return;

        BsonClassMap.RegisterClassMap<DoctorDiscussion>(classMap =>
        {
            classMap.AutoMap();
            classMap.SetIgnoreExtraElements(true);

            classMap.MapField("_tags")
                .SetElementName("tags");

            classMap.MapField("_comments")
                .SetElementName("comments");
        });
    }

    private static void RegisterConsultationNote()
    {
        if (BsonClassMap.IsClassMapRegistered(
                typeof(ConsultationNote)))
            return;

        BsonClassMap.RegisterClassMap<ConsultationNote>(classMap =>
        {
            classMap.AutoMap();
            classMap.SetIgnoreExtraElements(true);

            classMap.MapField("_recommendationsList")
                .SetElementName("recommendationsList");
        });
    }
}