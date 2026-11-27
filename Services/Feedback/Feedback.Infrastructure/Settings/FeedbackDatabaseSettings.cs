namespace Feedback.Infrastructure.Settings;

public sealed class FeedbackDatabaseSettings
{
    public const string SectionName = "FeedbackDatabase";

    public string ConnectionString { get; set; } =
        "mongodb://localhost:27017";

    public string DatabaseName { get; set; } =
        "ClinicFeedbackDB";

    public string PatientReviewsCollection { get; set; } =
        "PatientReviews";

    public string DoctorDiscussionsCollection { get; set; } =
        "DoctorDiscussions";

    public string ConsultationNotesCollection { get; set; } =
        "ConsultationNotes";
}