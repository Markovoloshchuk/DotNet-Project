using Feedback.Domain.Entities;
using Feedback.Infrastructure.Persistence;
using MongoDB.Driver;

namespace Feedback.Infrastructure.Seeding;

public sealed class PatientReviewSeeder(
    MongoDbContext context) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var existingReviews = await context.PatientReviews
            .Find(x =>
                x.ReviewId == 1 ||
                x.ReviewId == 2 ||
                (x.PatientId == 1 && x.DoctorId == 1) ||
                (x.PatientId == 2 && x.DoctorId == 2))
            .ToListAsync(ct);

        var reviews = new List<PatientReview>();

        if (!existingReviews.Any(x =>
            x.ReviewId == 1 ||
            (x.PatientId == 1 && x.DoctorId == 1)))
        {
            var review = new PatientReview(
                reviewId: 1,
                patientId: 1,
                doctorId: 1,
                rating: 5,
                comment: "Дуже ввічливий лікар, професійний огляд. Рекомендую!",
                tags: new[]
                {
                    "пунктуальність",
                    "професіоналізм"
                },
                createdAt: DateTime.Parse("2026-11-20T11:30:00Z"));

            review.SetClinicResponse(
                "Дякуємо за ваш відгук! Раді були допомогти.",
                DateTime.Parse("2026-11-20T14:00:00Z"));

            for (var i = 0; i < 12; i++)
                review.AddHelpfulVote();

            reviews.Add(review);
        }

        if (!existingReviews.Any(x =>
            x.ReviewId == 2 ||
            (x.PatientId == 2 && x.DoctorId == 2)))
        {
            reviews.Add(new PatientReview(
                reviewId: 2,
                patientId: 2,
                doctorId: 2,
                rating: 4,
                comment: "Консультація пройшла добре, але довелося трохи зачекати під кабінетом.",
                verifiedVisit: true,
                tags: new[] { "комфорт" },
                createdAt: DateTime.Parse("2026-11-21T13:10:00Z")));
        }

        if (reviews.Count == 0)
            return;

        await context.PatientReviews.InsertManyAsync(
            reviews,
            cancellationToken: ct);
    }
}