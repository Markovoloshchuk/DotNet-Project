USE ClinicAppointmentsDB;
GO

-- Ідемпотентне заповнення пацієнтів
IF NOT EXISTS (SELECT * FROM Patients WHERE PhoneNumber = '+380501112233')
    INSERT INTO Patients (FullName, PhoneNumber, Email, DateOfBirth) 
    VALUES (N'Шевченко Петро Іванович', '+380501112233', 'shevchenko@gmail.com', '1985-04-12');

IF NOT EXISTS (SELECT * FROM Patients WHERE PhoneNumber = '+380672223344')
    INSERT INTO Patients (FullName, PhoneNumber, Email, DateOfBirth) 
    VALUES (N'Коваленко Марія Степанівна', '+380672223344', 'kovalenko@ukr.net', '1992-09-25');
GO

-- Ідемпотентне заповнення лікарів
IF NOT EXISTS (SELECT * FROM Doctors WHERE FullName = N'Мельник Олег Васильович')
    INSERT INTO Doctors (FullName, Specialization, CabinetNumber, IsActive) 
    VALUES (N'Мельник Олег Васильович', N'Терапевт', '101', 1);

IF NOT EXISTS (SELECT * FROM Doctors WHERE FullName = N'Бондар Олена Петрівна')
    INSERT INTO Doctors (FullName, Specialization, CabinetNumber, IsActive) 
    VALUES (N'Бондар Олена Петрівна', N'Кардіолог', '204', 1);
GO

-- Ідемпотентне заповнення візитів
IF NOT EXISTS (SELECT * FROM Appointments WHERE AppointmentId = 1)
    SET IDENTITY_INSERT Appointments ON;
    INSERT INTO Appointments (AppointmentId, PatientId, DoctorId, AppointmentDate, Status, TotalAmount, CreatedBy) 
    VALUES (1, 1, 1, '2026-11-20 10:00:00', 'Scheduled', 600.00, 'Admin');
    SET IDENTITY_INSERT Appointments OFF;

IF NOT EXISTS (SELECT * FROM Appointments WHERE AppointmentId = 2)
    SET IDENTITY_INSERT Appointments ON;
    INSERT INTO Appointments (AppointmentId, PatientId, DoctorId, AppointmentDate, Status, TotalAmount, CreatedBy) 
    VALUES (2, 2, 2, '2026-11-21 12:30:00', 'Completed', 950.00, 'Admin');
    SET IDENTITY_INSERT Appointments OFF;
GO

-- Ідемпотентне заповнення рецептів
IF NOT EXISTS (SELECT * FROM Prescriptions WHERE AppointmentId = 1)
    INSERT INTO Prescriptions (AppointmentId, Diagnosis, MedicinesRecommendations) 
    VALUES (1, N'ГРВІ, гострий фарингіт', N'Віт. C 1000мг, Піопаз, рясне пиття');

IF NOT EXISTS (SELECT * FROM Prescriptions WHERE AppointmentId = 2)
    INSERT INTO Prescriptions (AppointmentId, Diagnosis, MedicinesRecommendations) 
    VALUES (2, N'Артеріальна гіпертензія I ступеня', N'Конкор 5мг вранці, контроль тиску');
GO

-- Ідемпотентне заповнення послуг візиту
IF NOT EXISTS (SELECT * FROM AppointmentServices WHERE AppointmentId = 1 AND ServiceId = 101)
    INSERT INTO AppointmentServices (AppointmentId, ServiceId, ServiceNameSnapshot, PriceSnapshot, Quantity) 
    VALUES (1, 101, N'Первинний огляд терапевта', 600.00, 1);

IF NOT EXISTS (SELECT * FROM AppointmentServices WHERE AppointmentId = 2 AND ServiceId = 102)
    INSERT INTO AppointmentServices (AppointmentId, ServiceId, ServiceNameSnapshot, PriceSnapshot, Quantity) 
    VALUES (2, 102, N'Консультація кардіолога', 650.00, 1);

IF NOT EXISTS (SELECT * FROM AppointmentServices WHERE AppointmentId = 2 AND ServiceId = 103)
    INSERT INTO AppointmentServices (AppointmentId, ServiceId, ServiceNameSnapshot, PriceSnapshot, Quantity) 
    VALUES (2, 103, N'Електрокардіографія (ЕКГ)', 300.00, 1);
GO