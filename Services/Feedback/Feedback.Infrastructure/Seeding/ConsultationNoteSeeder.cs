using Feedback.Domain.Entities;
using Feedback.Infrastructure.Persistence;
using MongoDB.Driver;

namespace Feedback.Infrastructure.Seeding;

public sealed class ConsultationNoteSeeder(
    MongoDbContext context) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var existing = await context.ConsultationNotes
            .Find(x =>
                x.NoteId == 1001 ||
                x.NoteId == 1002 ||
                x.AppointmentId == 1 ||
                x.AppointmentId == 2)
            .ToListAsync(ct);

        var notes = new List<ConsultationNote>();

        if (!existing.Any(x =>
            x.NoteId == 1001 ||
            x.AppointmentId == 1))
        {
            notes.Add(new ConsultationNote(
                noteId: 1001,
                appointmentId: 1,
                summary: "Пацієнт скаржиться на біль у горлі. Призначено симптоматичне лікування.",
                recommendations: new[]
                {
                    "Спокій",
                    "Тепле пиття",
                    "Вітамін C"
                }));
        }

        if (!existing.Any(x =>
            x.NoteId == 1002 ||
            x.AppointmentId == 2))
        {
            notes.Add(new ConsultationNote(
                noteId: 1002,
                appointmentId: 2,
                summary: "Тиск 140/90 мм рт. ст. Тони серця дещо приглушені.",
                recommendations: new[]
                {
                    "Щоденний контроль тиску",
                    "Обмеження солі"
                }));
        }

        if (notes.Count == 0)
            return;

        await context.ConsultationNotes.InsertManyAsync(
            notes,
            cancellationToken: ct);
    }
}