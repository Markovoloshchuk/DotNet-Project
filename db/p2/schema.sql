-- Створення структури бази даних для Проєкту №2 (Сервіс «Каталог послуг та обладнання» - EF Core Code First аналог DDL)

CREATE DATABASE ClinicCatalogDB;
GO

USE ClinicCatalogDB;
GO

-- 1. Категорії послуг (Зв'язок 1:N з MedicalServices)
CREATE TABLE ServiceCategories (
    CategoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 2. Медичне обладнання
CREATE TABLE Equipment (
    EquipmentId BIGINT IDENTITY(1,1) PRIMARY KEY,
    EquipmentName NVARCHAR(150) NOT NULL,
    SerialNumber VARCHAR(50) NOT NULL UNIQUE,
    PurchaseDate DATE NOT NULL,
    IsOperational BIT NOT NULL DEFAULT 1
);

-- 3. Медичні послуги (Головна сутність каталогу)
CREATE TABLE MedicalServices (
    ServiceId BIGINT IDENTITY(1,1) PRIMARY KEY,
    CategoryId BIGINT NOT NULL,
    EquipmentId BIGINT NULL,
    ServiceName NVARCHAR(150) NOT NULL,
    Price DECIMAL(18,2) NOT NULL CHECK (Price >= 0),
    DurationMinutes INT NOT NULL CHECK (DurationMinutes > 0),
    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_MedicalServices_Categories FOREIGN KEY (CategoryId) REFERENCES ServiceCategories(CategoryId) ON DELETE CASCADE,
    CONSTRAINT FK_MedicalServices_Equipment FOREIGN KEY (EquipmentId) REFERENCES Equipment(EquipmentId) ON DELETE SET NULL
);

-- 4. Деталі послуги (Зв'язок 1:1 з MedicalServices)
CREATE TABLE ServiceDetails (
    ServiceDetailId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ServiceId BIGINT NOT NULL UNIQUE,
    PreparationInstructions NVARCHAR(MAX) NULL,
    Contraindications NVARCHAR(MAX) NULL,
    
    CONSTRAINT FK_ServiceDetails_MedicalServices FOREIGN KEY (ServiceId) REFERENCES MedicalServices(ServiceId) ON DELETE CASCADE
);

-- 5. Комплексні пакети послуг
CREATE TABLE ServicePackages (
    PackageId BIGINT IDENTITY(1,1) PRIMARY KEY,
    PackageName NVARCHAR(150) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL CHECK (TotalPrice >= 0),
    DiscountPercentage DECIMAL(5,2) NOT NULL DEFAULT 0.00 CHECK (DiscountPercentage >= 0 AND DiscountPercentage <= 100)
);

-- 6. Проміжна таблиця для зв'язку M:N між MedicalServices та ServicePackages з власним полем (Quantity та CustomNotes)
CREATE TABLE PackageServices (
    PackageId BIGINT NOT NULL,
    ServiceId BIGINT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1 CHECK (Quantity > 0),
    CustomNotes NVARCHAR(200) NULL, -- власне поле в M:N зв'язку

    CONSTRAINT PK_PackageServices PRIMARY KEY (PackageId, ServiceId),
    CONSTRAINT FK_PackageServices_Packages FOREIGN KEY (PackageId) REFERENCES ServicePackages(PackageId) ON DELETE CASCADE,
    CONSTRAINT FK_PackageServices_Services FOREIGN KEY (ServiceId) REFERENCES MedicalServices(ServiceId) ON DELETE CASCADE
);

-- Створення індексів для оптимізації пошуку
CREATE INDEX IX_MedicalServices_CategoryId ON MedicalServices(CategoryId);
CREATE INDEX IX_MedicalServices_Price ON MedicalServices(Price);
CREATE UNIQUE INDEX IX_Equipment_SerialNumber ON Equipment(SerialNumber);