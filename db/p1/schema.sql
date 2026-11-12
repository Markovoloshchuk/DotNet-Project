-- Створення структури бази даних для Проєкту №1 (Сервіс «Записи та візити»)

CREATE DATABASE ClinicAppointmentsDB;
GO

USE ClinicAppointmentsDB;
GO

-- 1. Таблиця пацієнтів (Customer/Patient аналог)
CREATE TABLE Patients (
    PatientId BIGINT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL UNIQUE,
    Email NVARCHAR(100) NULL,
    DateOfBirth DATE NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 2. Таблиця лікарів
CREATE TABLE Doctors (
    DoctorId BIGINT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Specialization NVARCHAR(100) NOT NULL,
    CabinetNumber VARCHAR(20) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 3. Таблиця візитів (Order аналог) -> Зв'язок 1:N з Patients та Doctors
CREATE TABLE Appointments (
    AppointmentId BIGINT IDENTITY(1,1) PRIMARY KEY,
    PatientId BIGINT NOT NULL,
    DoctorId BIGINT NOT NULL,
    AppointmentDate DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Scheduled', -- Scheduled, Completed, Cancelled
    TotalAmount DECIMAL(10, 2) NOT NULL CHECK (TotalAmount >= 0),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_Appointments_Patients FOREIGN KEY (PatientId) REFERENCES Patients(PatientId) ON DELETE CASCADE,
    CONSTRAINT FK_Appointments_Doctors FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId)
);

-- 4. Таблиця медичних рецептів/призначень -> Зв'язок 1:1 з Appointments
CREATE TABLE Prescriptions (
    PrescriptionId BIGINT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId BIGINT NOT NULL UNIQUE,
    Diagnosis NVARCHAR(500) NOT NULL,
    MedicinesRecommendations NVARCHAR(MAX) NULL,
    IssuedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Prescriptions_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId) ON DELETE CASCADE
);

-- 5. Проміжна таблиця послуг у візиті -> Зв'язок M:N між Appointments та послугами каталогу (дубльовані дані ціни та назви)
CREATE TABLE AppointmentServices (
    AppointmentServiceId BIGINT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId BIGINT NOT NULL,
    ServiceId BIGINT NOT NULL, -- посилання на каталог (Проєкт №2) через ID
    ServiceNameSnapshot NVARCHAR(150) NOT NULL, -- дублювання назви послуги
    PriceSnapshot DECIMAL(10, 2) NOT NULL CHECK (PriceSnapshot >= 0),
    Quantity INT NOT NULL DEFAULT 1 CHECK (Quantity > 0),

    CONSTRAINT FK_AppointmentServices_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId) ON DELETE CASCADE
);

-- Створення індексів для оптимізації пошуку
CREATE INDEX IX_Appointments_PatientId ON Appointments(PatientId);
CREATE INDEX IX_Appointments_DoctorId ON Appointments(DoctorId);
CREATE INDEX IX_Appointments_Date ON Appointments(AppointmentDate);
CREATE INDEX IX_Patients_PhoneNumber ON Patients(PhoneNumber);