using Feedback.Domain.Common;
using Feedback.Domain.Entities;
using Feedback.Domain.ValueObjects;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

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

            RegisterBaseEntity();
            RegisterClinicResponse();
            RegisterDiscussionComment();
            RegisterPatientReview();
            RegisterDoctorDiscussion();
            RegisterConsultationNote();

            _registered = true;
        }
    }

    private static void RegisterBaseEntity()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
            return;

        BsonClassMap.RegisterClassMap<BaseEntity>(classMap =>
        {
            classMap.MapIdMember(x => x.Id)
                .SetSerializer(
                    new StringSerializer(BsonType.ObjectId));

            classMap.MapProperty(nameof(BaseEntity.CreatedAt))
                .SetElementName("createdAt");

            classMap.MapProperty(nameof(BaseEntity.UpdatedAt))
                .SetElementName("updatedAt");

            classMap.MapProperty(nameof(BaseEntity.Version))
                .SetElementName("version");
        });
    }

    private static void RegisterClinicResponse()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(ClinicResponse)))
            return;

        BsonClassMap.RegisterClassMap<ClinicResponse>(classMap =>
        {
            classMap.MapProperty(nameof(ClinicResponse.RespondedAt))
                .SetElementName("respondedAt");

            classMap.MapProperty(nameof(ClinicResponse.Message))
                .SetElementName("message");

            classMap.MapCreator(
                x => new ClinicResponse(
                    x.RespondedAt,
                    x.Message));

            classMap.SetIgnoreExtraElements(true);
        });
    }

    private static void RegisterDiscussionComment()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(DiscussionComment)))
            return;

        BsonClassMap.RegisterClassMap<DiscussionComment>(classMap =>
        {
            classMap.MapProperty(nameof(DiscussionComment.CommentId))
                .SetElementName("commentId");

            classMap.MapProperty(nameof(DiscussionComment.DoctorRefId))
                .SetElementName("doctorRefId");

            classMap.MapProperty(nameof(DiscussionComment.Text))
                .SetElementName("text");

            classMap.MapProperty(nameof(DiscussionComment.CreatedAt))
                .SetElementName("createdAt");

            classMap.SetIgnoreExtraElements(true);
        });
    }

    private static void RegisterPatientReview()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(PatientReview)))
            return;

        BsonClassMap.RegisterClassMap<PatientReview>(classMap =>
        {
            classMap.MapProperty(nameof(PatientReview.ReviewId))
                .SetElementName("reviewId");

            classMap.MapProperty(nameof(PatientReview.PatientId))
                .SetElementName("patientId");

            classMap.MapProperty(nameof(PatientReview.DoctorId))
                .SetElementName("doctorId");

            classMap.MapProperty(nameof(PatientReview.Rating))
                .SetElementName("rating");

            classMap.MapProperty(nameof(PatientReview.Comment))
                .SetElementName("comment");

            classMap.MapProperty(nameof(PatientReview.ClinicResponse))
                .SetElementName("clinicResponse");

            classMap.MapProperty(nameof(PatientReview.HelpfulVotes))
                .SetElementName("helpfulVotes");

            classMap.MapProperty(nameof(PatientReview.VerifiedVisit))
                .SetElementName("verifiedVisit");

            classMap.MapField("_tags")
                .SetElementName("tags");

            classMap.SetIgnoreExtraElements(true);
        });
    }

    private static void RegisterDoctorDiscussion()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(DoctorDiscussion)))
            return;

        BsonClassMap.RegisterClassMap<DoctorDiscussion>(classMap =>
        {
            classMap.MapProperty(nameof(DoctorDiscussion.DiscussionId))
                .SetElementName("discussionId");

            classMap.MapProperty(nameof(DoctorDiscussion.Title))
                .SetElementName("title");

            classMap.MapProperty(nameof(DoctorDiscussion.AuthorDoctorId))
                .SetElementName("authorDoctorId");

            classMap.MapProperty(nameof(DoctorDiscussion.Content))
                .SetElementName("content");

            classMap.MapField("_tags")
                .SetElementName("tags");

            classMap.MapField("_comments")
                .SetElementName("comments");

            classMap.SetIgnoreExtraElements(true);
        });
    }

    private static void RegisterConsultationNote()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(ConsultationNote)))
            return;

        BsonClassMap.RegisterClassMap<ConsultationNote>(classMap =>
        {
            classMap.MapProperty(nameof(ConsultationNote.NoteId))
                .SetElementName("noteId");

            classMap.MapProperty(nameof(ConsultationNote.AppointmentId))
                .SetElementName("appointmentId");

            classMap.MapProperty(nameof(ConsultationNote.Summary))
                .SetElementName("summary");

            classMap.MapField("_recommendationsList")
                .SetElementName("recommendationsList");

            classMap.SetIgnoreExtraElements(true);
        });
    }
}