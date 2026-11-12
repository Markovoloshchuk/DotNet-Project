USE ClinicAppointmentsDB;
GO

-- Початкові дані пацієнтів
INSERT INTO Patients (FullName, PhoneNumber, Email, DateOfBirth) VALUES
(N'Шевченко Петро Іванович', '+380501112233', 'shevchenko@gmail.com', '1985-04-12'),
(N'Коваленко Марія Степанівна', '+380672223344', 'kovalenko@ukr.net', '1992-09-25');

-- Початкові дані лікарів
INSERT INTO Doctors (FullName, Specialization, CabinetNumber, IsActive) VALUES
(N'Мельник Олег Васильович', N'Терапевт', '101', 1),
(N'Бондар Олена Петрівна', N'Кардіолог', '204', 1);

-- Візити (Appointments)
INSERT INTO Appointments (PatientId, DoctorId, AppointmentDate, Status, TotalAmount) VALUES
(1, 1, '2026-11-20 10:00:00', 'Scheduled', 600.00),
(2, 2, '2026-11-21 12:30:00', 'Completed', 950.00);

-- Рецепти (1:1 з Appointments)
INSERT INTO Prescriptions (AppointmentId, Diagnosis, MedicinesRecommendations) VALUES
(1, N'ГРВІ, гострий фарингіт', N'Віт. C 1000мг, Піопаз, рясне пиття'),
(2, N'Артеріальна гіпертензія I ступеня', N'Конкор 5мг вранці, контроль тиску');

-- Позиції послуг у візитах (M:N)
INSERT INTO AppointmentServices (AppointmentId, ServiceId, ServiceNameSnapshot, PriceSnapshot, Quantity) VALUES
(1, 101, N'Первинний огляд терапевта', 600.00, 1),
(2, 102, N'Консультація кардіолога', 650.00, 1),
(2, 103, N'Електрокардіографія (ЕКГ)', 300.00, 1);