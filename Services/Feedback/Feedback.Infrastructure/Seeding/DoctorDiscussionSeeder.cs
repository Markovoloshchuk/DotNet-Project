using Feedback.Domain.Entities;
using Feedback.Infrastructure.Persistence;
using MongoDB.Driver;

namespace Feedback.Infrastructure.Seeding;

public sealed class DoctorDiscussionSeeder(
    MongoDbContext context) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var existing = await context.DoctorDiscussions
            .Find(x => x.DiscussionId == 101)
            .AnyAsync(ct);

        if (existing)
            return;

        var discussion = new DoctorDiscussion(
            discussionId: 101,
            title: "Сучасні підходи до лікування артеріальної гіпертензії у пацієнтів літнього віку",
            authorDoctorId: 2,
            content: "Колеги, які комбінації препаратів ви вважаєте найефективнішими у першій лінії терапії?",
            tags: new[]
            {
                "кардіологія",
                "терапія",
                "протоколи"
            },
            createdAt: DateTime.Parse("2026-11-15T09:00:00Z"));

        discussion.AddComment(new DiscussionComment(
            commentId: 1,
            doctorRefId: 1,
            text: "Використовуємо інгібітори АПФ в комбінації з антагоністами кальцію.",
            createdAt: DateTime.Parse("2026-11-15T10:15:00Z")));

        await context.DoctorDiscussions.InsertOneAsync(
            discussion,
            cancellationToken: ct);
    }
}