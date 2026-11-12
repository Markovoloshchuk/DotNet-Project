USE ClinicCatalogDB;
GO

-- Ідемпотентні категорії
IF NOT EXISTS (SELECT * FROM ServiceCategories WHERE CategoryName = N'Діагностика')
    INSERT INTO ServiceCategories (CategoryName, Description) VALUES (N'Діагностика', N'Ультразвукові, лабораторні та функціональні дослідження');

IF NOT EXISTS (SELECT * FROM ServiceCategories WHERE CategoryName = N'Консультації фахівців')
    INSERT INTO ServiceCategories (CategoryName, Description) VALUES (N'Консультації фахівців', N'Первинні та повторні прийоми лікарів');

IF NOT EXISTS (SELECT * FROM ServiceCategories WHERE CategoryName = N'Фізіотерапія та реабілітація')
    INSERT INTO ServiceCategories (CategoryName, Description) VALUES (N'Фізіотерапія та реабілітація', N'Відновлювальні процедури та масаж');
GO

-- Ідемпотентне обладнання
IF NOT EXISTS (SELECT * FROM Equipment WHERE SerialNumber = 'PH-350-9981')
    INSERT INTO Equipment (EquipmentName, SerialNumber, PurchaseDate, IsOperational) VALUES (N'УЗД апарат Philips ClearVue 350', 'PH-350-9981', '2023-05-10', 1);

IF NOT EXISTS (SELECT * FROM Equipment WHERE SerialNumber = 'ECG-12CH-442')
    INSERT INTO Equipment (EquipmentName, SerialNumber, PurchaseDate, IsOperational) VALUES (N'Електрокардіограф 12-канальний', 'ECG-12CH-442', '2024-01-15', 1);
GO

-- Ідемпотентні медичні послуги
IF NOT EXISTS (SELECT * FROM MedicalServices WHERE ServiceName = N'Ультразвукове дослідження органів черевної порожнини')
    INSERT INTO MedicalServices (CategoryId, EquipmentId, ServiceName, Price, DurationMinutes, IsActive) 
    VALUES (1, 1, N'Ультразвукове дослідження органів черевної порожнини', 700.00, 30, 1);

IF NOT EXISTS (SELECT * FROM MedicalServices WHERE ServiceName = N'Первинний огляд терапевта')
    INSERT INTO MedicalServices (CategoryId, EquipmentId, ServiceName, Price, DurationMinutes, IsActive) 
    VALUES (2, NULL, N'Первинний огляд терапевта', 600.00, 20, 1);

IF NOT EXISTS (SELECT * FROM MedicalServices WHERE ServiceName = N'Консультація кардіолога')
    INSERT INTO MedicalServices (CategoryId, EquipmentId, ServiceName, Price, DurationMinutes, IsActive) 
    VALUES (2, NULL, N'Консультація кардіолога', 650.00, 30, 1);

IF NOT EXISTS (SELECT * FROM MedicalServices WHERE ServiceName = N'Електрокардіографія (ЕКГ) з розшифровкою')
    INSERT INTO MedicalServices (CategoryId, EquipmentId, ServiceName, Price, DurationMinutes, IsActive) 
    VALUES (1, 2, N'Електрокардіографія (ЕКГ) з розшифровкою', 300.00, 15, 1);
GO

-- Ідемпотентні пакети
IF NOT EXISTS (SELECT * FROM ServicePackages WHERE PackageName = N'Кардіокомплекс повний')
    INSERT INTO ServicePackages (PackageName, TotalPrice, DiscountPercentage) VALUES (N'Кардіокомплекс повний', 850.00, 15.00);

IF NOT EXISTS (SELECT * FROM ServicePackages WHERE PackageName = N'Базовий профілактичний')
    INSERT INTO ServicePackages (PackageName, TotalPrice, DiscountPercentage) VALUES (N'Базовий профілактичний', 1100.00, 10.00);
GO