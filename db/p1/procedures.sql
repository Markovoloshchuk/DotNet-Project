USE ClinicAppointmentsDB;
GO

-- 1. Створення нового пацієнта (Create)
CREATE PROCEDURE sp_CreatePatient
    @FullName NVARCHAR(150),
    @PhoneNumber VARCHAR(20),
    @Email NVARCHAR(100),
    @DateOfBirth DATE
AS
BEGIN
    INSERT INTO Patients (FullName, PhoneNumber, Email, DateOfBirth)
    VALUES (@FullName, @PhoneNumber, @Email, @DateOfBirth);
    
    SELECT SCOPE_IDENTITY() AS PatientId;
END;
GO

-- 2. Отримання детальної інформації про візит за ID (Read)
CREATE PROCEDURE sp_GetAppointmentDetails
    @AppointmentId BIGINT
AS
BEGIN
    SELECT 
        a.AppointmentId,
        a.AppointmentDate,
        a.Status,
        a.TotalAmount,
        p.FullName AS PatientName,
        p.PhoneNumber AS PatientPhone,
        d.FullName AS DoctorName,
        d.Specialization,
        pr.Diagnosis,
        pr.MedicinesRecommendations
    FROM Appointments a
    INNER JOIN Patients p ON a.PatientId = p.PatientId
    INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
    LEFT JOIN Prescriptions pr ON a.AppointmentId = pr.AppointmentId
    WHERE a.AppointmentId = @AppointmentId;
END;
GO

-- 3. Транзакційне створення візиту з послугами та перевіркою
CREATE PROCEDURE sp_CreateAppointmentWithServices
    @PatientId BIGINT,
    @DoctorId BIGINT,
    @AppointmentDate DATETIME2,
    @TotalAmount DECIMAL(10, 2)
AS
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        INSERT INTO Appointments (PatientId, DoctorId, AppointmentDate, Status, TotalAmount)
        VALUES (@PatientId, @DoctorId, @AppointmentDate, 'Scheduled', @TotalAmount);

        DECLARE @NewAppointmentId BIGINT = SCOPE_IDENTITY();

        -- Повертаємо створений ID для подальшого додавання позицій у код додатку
        SELECT @NewAppointmentId AS AppointmentId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 4. Зміна статусу візиту (транзакційна бізнес-операція)
CREATE PROCEDURE sp_UpdateAppointmentStatus
    @AppointmentId BIGINT,
    @NewStatus NVARCHAR(50)
AS
BEGIN
    UPDATE Appointments
    SET Status = @NewStatus
    WHERE AppointmentId = @AppointmentId;
    
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Візит не знайдено', 16, 1);
    END
END;
GO

-- 5. Додавання послуги до візиту (M:N зв'язок)
CREATE PROCEDURE sp_AddServiceToAppointment
    @AppointmentId BIGINT,
    @ServiceId BIGINT,
    @ServiceNameSnapshot NVARCHAR(150),
    @PriceSnapshot DECIMAL(10, 2),
    @Quantity INT
AS
BEGIN
    INSERT INTO AppointmentServices (AppointmentId, ServiceId, ServiceNameSnapshot, PriceSnapshot, Quantity)
    VALUES (@AppointmentId, @ServiceId, @ServiceNameSnapshot, @PriceSnapshot, @Quantity);
END;
GO