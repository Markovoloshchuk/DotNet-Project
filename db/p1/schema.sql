-- Створення структури бази даних для Проєкту №1 (Сервіс «Записи та візити»)

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ClinicAppointmentsDB')
BEGIN
    CREATE DATABASE ClinicAppointmentsDB;
END
GO

USE ClinicAppointmentsDB;
GO

-- Перевірка та створення таблиці пацієнтів
IF OBJECT_ID('Patients', 'U') IS NULL
BEGIN
    CREATE TABLE Patients (
        PatientId BIGINT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        PhoneNumber VARCHAR(20) NOT NULL UNIQUE,
        Email NVARCHAR(100) NULL,
        DateOfBirth DATE NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
GO

-- Перевірка та створення таблиці лікарів
IF OBJECT_ID('Doctors', 'U') IS NULL
BEGIN
    CREATE TABLE Doctors (
        DoctorId BIGINT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Specialization NVARCHAR(100) NOT NULL,
        CabinetNumber VARCHAR(20) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

-- Перевірка та створення таблиці візитів (з аудитними колонками та rowversion для оптимістичної конкурентності)
IF OBJECT_ID('Appointments', 'U') IS NULL
BEGIN
    CREATE TABLE Appointments (
        AppointmentId BIGINT IDENTITY(1,1) PRIMARY KEY,
        PatientId BIGINT NOT NULL,
        DoctorId BIGINT NOT NULL,
        AppointmentDate DATETIME2 NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Scheduled', -- Scheduled, Completed, Cancelled
        TotalAmount DECIMAL(10, 2) NOT NULL CHECK (TotalAmount >= 0),
        
        -- Аудитні колонки та м'яке видалення
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100) NOT NULL DEFAULT 'System',
        UpdatedAt DATETIME2 NULL,
        UpdatedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        
        -- Поле для оптимістичної конкурентності
        RowVersion ROWVERSION,

        CONSTRAINT FK_Appointments_Patients FOREIGN KEY (PatientId) REFERENCES Patients(PatientId) ON DELETE CASCADE,
        CONSTRAINT FK_Appointments_Doctors FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId)
    );
END
GO

-- Перевірка та створення таблиці медичних рецептів (Зв'язок 1:1)
IF OBJECT_ID('Prescriptions', 'U') IS NULL
BEGIN
    CREATE TABLE Prescriptions (
        PrescriptionId BIGINT IDENTITY(1,1) PRIMARY KEY,
        AppointmentId BIGINT NOT NULL UNIQUE,
        Diagnosis NVARCHAR(500) NOT NULL,
        MedicinesRecommendations NVARCHAR(MAX) NULL,
        IssuedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_Prescriptions_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId) ON DELETE CASCADE
    );
END
GO

-- Перевірка та створення проміжної таблиці послуг (Зв'язок M:N)
IF OBJECT_ID('AppointmentServices', 'U') IS NULL
BEGIN
    CREATE TABLE AppointmentServices (
        AppointmentServiceId BIGINT IDENTITY(1,1) PRIMARY KEY,
        AppointmentId BIGINT NOT NULL,
        ServiceId BIGINT NOT NULL, 
        ServiceNameSnapshot NVARCHAR(150) NOT NULL, 
        PriceSnapshot DECIMAL(10, 2) NOT NULL CHECK (PriceSnapshot >= 0),
        Quantity INT NOT NULL DEFAULT 1 CHECK (Quantity > 0),

        CONSTRAINT FK_AppointmentServices_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId) ON DELETE CASCADE
    );
END
GO

-- Створення індексів із поясненням призначення під конкретні запити
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Appointments_PatientId')
    CREATE INDEX IX_Appointments_PatientId ON Appointments(PatientId);
    /* Використовується для швидкого пошуку та фільтрації всіх візитів конкретного пацієнта в історії */

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Appointments_DoctorId')
    CREATE INDEX IX_Appointments_DoctorId ON Appointments(DoctorId);
    /* Використовується для побудови розкладу прийомів та пошуку навантаження конкретного лікаря */

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Appointments_Date')
    CREATE INDEX IX_Appointments_Date ON Appointments(AppointmentDate);
    /* Використовується для вибірки візитів на конкретну дату або діапазон дат */

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Patients_PhoneNumber')
    CREATE INDEX IX_Patients_PhoneNumber ON Patients(PhoneNumber);
    /* Використовується для швидкої ідентифікації пацієнта за номером телефону при дзвінку чи записі */
GO