// Створення бази даних та колекцій для Проєкту №3 (MongoDB: Відгуки та обговорення)

use ClinicFeedbackDB;

// 1. Колекція відгуків пацієнтів (Гібридний підхід + Embedding для реакцій/лайків)
db.createCollection("PatientReviews", {
   validator: {
      $jsonSchema: {
         bsonType: "object",
         required: ["reviewId", "patientId", "doctorId", "rating", "comment", "createdAt"],
         properties: {
            reviewId: { bsonType: "long" },
            patientId: { bsonType: "long" },
            doctorId: { bsonType: "long" },
            rating: { bsonType: "int", minimum: 1, maximum: 5 },
            comment: { bsonType: "string" },
            createdAt: { bsonType: "date" }
         }
      }
   }
});

// 2. Колекція обговорень діагнозів та консультацій (Referencing + Вкладені масиви коментарів)
db.createCollection("DoctorDiscussions", {
   validator: {
      $jsonSchema: {
         bsonType: "object",
         required: ["discussionId", "title", "authorDoctorId", "content", "tags"],
         properties: {
            discussionId: { bsonType: "long" },
            title: { bsonType: "string" },
            authorDoctorId: { bsonType: "long" },
            content: { bsonType: "string" },
            tags: { bsonType: "array", items: { bsonType: "string" } }
         }
      }
   }
});

// 3. Колекція консультаційних заміток (Денормалізована структура для швидкого читання)
db.createCollection("ConsultationNotes", {
   validator: {
      $jsonSchema: {
         bsonType: "object",
         required: ["noteId", "appointmentId", "summary"],
         properties: {
            noteId: { bsonType: "long" },
            appointmentId: { bsonType: "long" },
            summary: { bsonType: "string" }
         }
      }
   }
});

// Створення індексів для оптимізації пошуку та агрегацій
db.PatientReviews.createIndex({ doctorId: 1, rating: -1 });
db.PatientReviews.createIndex({ reviewId: 1 }, { unique: true });
db.DoctorDiscussions.createIndex({ tags: 1 });
db.ConsultationNotes.createIndex({ appointmentId: 1 }, { unique: true });

print("Колекції MongoDB успішно створено та налаштовано.");