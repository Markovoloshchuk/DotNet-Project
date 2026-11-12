// Початкові тестові дані (Seed) для MongoDB (Проєкт №3)

use ClinicFeedbackDB;

// Очищення попередніх даних (ідемпотентність)
db.PatientReviews.deleteMany({});
db.DoctorDiscussions.deleteMany({});
db.ConsultationNotes.deleteMany({});

// 1. Вставка відгуків пацієнтів (демонстрація embedding: вбудовані піддокументи відповідей адміністрації та реакцій)
db.PatientReviews.insertMany([
   {
      reviewId: NumberLong(1),
      patientId: NumberLong(1),
      doctorId: NumberLong(1),
      rating: 5,
      comment: "Дуже ввічливий лікар, професійний огляд. Рекомендую!",
      createdAt: new Date("2026-11-20T11:30:00Z"),
      // Вбудований піддокумент відповіді клініки (Embedding)
      clinicResponse: {
         respondedAt: new Date("2026-11-20T14:00:00Z"),
         message: "Дякуємо за ваш відгук! Раді були допомогти."
      },
      // Масив вбудованих тегів/реакцій (демонстрація гнучкості NoSQL)
      helpfulVotes: 12,
      tags: ["пунктуальність", "професіоналізм"]
   },
   {
      reviewId: NumberLong(2),
      patientId: NumberLong(2),
      doctorId: NumberLong(2),
      rating: 4,
      comment: "Консультація пройшла добре, але довелося трохи зачекати під кабінетом.",
      createdAt: new Date("2026-11-21T13:10:00Z"),
      // Гнучкість схеми: у цього документа немає clinicResponse, але є додаткове поле погодження
      verifiedVisit: true,
      tags: ["комфорт"]
   }
]);

// 2. Вставка обговорень (демонстрація вкладених масивів коментарів)
db.DoctorDiscussions.insertMany([
   {
      discussionId: NumberLong(101),
      title: "Сучасні підходи до лікування артеріальної гіпертензії у пацієнтів літнього віку",
      authorDoctorId: NumberLong(2),
      content: "Колеги, які комбінації препаратів ви вважаєте найефективнішими у першій лінії терапії?",
      tags: ["кардіологія", "терапія", "протоколи"],
      createdAt: new Date("2026-11-15T09:00:00Z"),
      // Вкладений масив коментарів (Embedding)
      comments: [
         {
            commentId: NumberLong(1),
            doctorRefId: NumberLong(1),
            text: "Використовуємо інгібітори АПФ в комбінації з антагоністами кальцію.",
            createdAt: new Date("2026-11-15T10:15:00Z")
         }
      ]
   }
]);

// 3. Вставка консультаційних заміток (деми: денормалізовані дані)
db.ConsultationNotes.insertMany([
   {
      noteId: NumberLong(1001),
      appointmentId: NumberLong(1),
      summary: "Пацієнт скаржиться на біль у горлі. Призначено симптоматичне лікування.",
      recommendationsList: ["Спокій", "Тепле пиття", "Вітамін C"]
   },
   {
      noteId: NumberLong(1002),
      appointmentId: NumberLong(2),
      summary: "Тиск 140/90 мм рт. ст. Тони серця дещо приглушені.",
      recommendationsList: ["Щоденний контроль тиску", "Обмеження солі"]
   }
]);

// Приклад Aggregate Pipeline запиту: розрахунок середнього рейтингу по кожному лікарю
print("Результати агрегації (Середній рейтинг лікарів):");
const aggregationResult = db.PatientReviews.aggregate([
   {
      $group: {
         _id: "$doctorId",
         averageRating: { $avg: "$rating" },
         totalReviews: { $sum: 1 }
      }
   }
]).toArray();

printjson(aggregationResult);