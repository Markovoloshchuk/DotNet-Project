// Ідемпотентні тестові дані (Seed) для MongoDB (Проєкт №3)

use ClinicFeedbackDB;

// 1. Вставка або оновлення відгуків пацієнтів (Ідемпотентно через reviewId)
db.PatientReviews.updateOne(
   { reviewId: NumberLong(1) },
   {
      $set: {
         patientId: NumberLong(1),
         doctorId: NumberLong(1),
         rating: 5,
         comment: "Дуже ввічливий лікар, професійний огляд. Рекомендую!",
         createdAt: new Date("2026-11-20T11:30:00Z"),
         clinicResponse: {
            respondedAt: new Date("2026-11-20T14:00:00Z"),
            message: "Дякуємо за ваш відгук! Раді були допомогти."
         },
         helpfulVotes: 12,
         tags: ["пунктуальність", "професіоналізм"]
      }
   },
   { upsert: true }
);

db.PatientReviews.updateOne(
   { reviewId: NumberLong(2) },
   {
      $set: {
         patientId: NumberLong(2),
         doctorId: NumberLong(2),
         rating: 4,
         comment: "Консультація пройшла добре, але довелося трохи зачекати під кабінетом.",
         createdAt: new Date("2026-11-21T13:10:00Z"),
         verifiedVisit: true,
         tags: ["комфорт"]
      }
   },
   { upsert: true }
);

// 2. Вставка або оновлення обговорень (Ідемпотентно через discussionId)
db.DoctorDiscussions.updateOne(
   { discussionId: NumberLong(101) },
   {
      $set: {
         title: "Сучасні підходи до лікування артеріальної гіпертензії у пацієнтів літнього віку",
         authorDoctorId: NumberLong(2),
         content: "Колеги, які комбінації препаратів ви вважаєте найефективнішими у першій лінії терапії?",
         tags: ["кардіологія", "терапія", "протоколи"],
         createdAt: new Date("2026-11-15T09:00:00Z"),
         comments: [
            {
               commentId: NumberLong(1),
               doctorRefId: NumberLong(1),
               text: "Використовуємо інгібітори АПФ в комбінації з антагоністами кальцію.",
               createdAt: new Date("2026-11-15T10:15:00Z")
            }
         ]
      }
   },
   { upsert: true }
);

// 3. Вставка або оновлення консультаційних заміток (Ідемпотентно через noteId)
db.ConsultationNotes.updateOne(
   { noteId: NumberLong(1001) },
   {
      $set: {
         appointmentId: NumberLong(1),
         summary: "Пацієнт скаржиться на біль у горлі. Призначено симптоматичне лікування.",
         recommendationsList: ["Спокій", "Тепле пиття", "Вітамін C"]
      }
   },
   { upsert: true }
);

db.ConsultationNotes.updateOne(
   { noteId: NumberLong(1002) },
   {
      $set: {
         appointmentId: NumberLong(2),
         summary: "Тиск 140/90 мм рт. ст. Тони серця дещо приглушені.",
         recommendationsList: ["Щоденний контроль тиску", "Обмеження солі"]
      }
   },
   { upsert: true }
);

print("Ідемпотентні seed-дані успішно застосовані.");