USE ClinicAppointmentsDB;
GO

-- 1. Створення нового пацієнта (Create)
CREATE OR ALTER PROCEDURE sp_CreatePatient
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
CREATE OR ALTER PROCEDURE sp_GetAppointmentDetails
    @AppointmentId BIGINT
AS
BEGIN
    SELECT 
        a.AppointmentId,
        a.AppointmentDate,
        a.Status,
        a.TotalAmount,
        a.CreatedAt,
        a.UpdatedAt,
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
    WHERE a.AppointmentId = @AppointmentId AND a.IsDeleted = 0;
END;
GO

-- 3. Транзакційне створення візиту з перевіркою
CREATE OR ALTER PROCEDURE sp_CreateAppointmentWithServices
    @PatientId BIGINT,
    @DoctorId BIGINT,
    @AppointmentDate DATETIME2,
    @TotalAmount DECIMAL(10, 2),
    @CreatedBy NVARCHAR(100) = 'System'
AS
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        INSERT INTO Appointments (PatientId, DoctorId, AppointmentDate, Status, TotalAmount, CreatedBy)
        VALUES (@PatientId, @DoctorId, @AppointmentDate, 'Scheduled', @TotalAmount, @CreatedBy);

        DECLARE @NewAppointmentId BIGINT = SCOPE_IDENTITY();

        SELECT @NewAppointmentId AS AppointmentId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 4. Транзакційна зміна статусу з бізнес-перевіркою та керованою помилкою
CREATE OR ALTER PROCEDURE sp_UpdateAppointmentStatus
    @AppointmentId BIGINT,
    @NewStatus NVARCHAR(50),
    @UpdatedBy NVARCHAR(100) = 'System'
AS
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @CurrentStatus NVARCHAR(50);
        
        -- Отримуємо поточний статус з блокуванням рядка
        SELECT @CurrentStatus = Status 
        FROM Appointments WITH (UPDLOCK, ROWLOCK) 
        WHERE AppointmentId = @AppointmentId AND IsDeleted = 0;

        IF @CurrentStatus IS NULL
        BEGIN
            RAISERROR('Візит не знайдено або він видалений.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Перевірка бізнес-правил переходів статусів
        IF @CurrentStatus = 'Completed'
        BEGIN
            RAISERROR('Неможливо змінити статус завершеного візиту.', 16, 2);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @CurrentStatus = 'Cancelled' AND @NewStatus = 'Completed'
        BEGIN
            RAISERROR('Неможливо завершити скасований візит.', 16, 3);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Оновлення статусу та аудитних полів
        UPDATE Appointments
        SET Status = @NewStatus,
            UpdatedAt = GETDATE(),
            UpdatedBy = @UpdatedBy
        WHERE AppointmentId = @AppointmentId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 5. Додавання послуги до візиту (M:N зв'язок)
CREATE OR ALTER PROCEDURE sp_AddServiceToAppointment
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