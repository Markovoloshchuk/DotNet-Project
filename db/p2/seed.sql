USE ClinicCatalogDB;
GO

-- Категорії послуг
INSERT INTO ServiceCategories (CategoryName, Description) VALUES
(N'Діагностика', N'Ультразвукові, лабораторні та функціональні дослідження'),
(N'Консультації фахівців', N'Первинні та повторні прийоми лікарів'),
(N'Фізіотерапія та реабілітація', N'Відновлювальні процедури та масаж');

-- Медичне обладнання
INSERT INTO Equipment (EquipmentName, SerialNumber, PurchaseDate, IsOperational) VALUES
(N'УЗД апарат Philips ClearVue 350', 'PH-350-9981', '2023-05-10', 1),
(N'Електрокардіограф 12-канальний', 'ECG-12CH-442', '2024-01-15', 1);

-- Медичні послуги
INSERT INTO MedicalServices (CategoryId, EquipmentId, ServiceName, Price, DurationMinutes, IsActive) VALUES
(1, 1, N'Ультразвукове дослідження органів черевної порожнини', 700.00, 30, 1),
(2, NULL, N'Первинний огляд терапевта', 600.00, 20, 1),
(2, NULL, N'Консультація кардіолога', 650.00, 30, 1),
(1, 2, N'Електрокардіографія (ЕКГ) з розшифровкою', 300.00, 15, 1);

-- Деталі послуг (Зв'язок 1:1)
INSERT INTO ServiceDetails (ServiceId, PreparationInstructions, Contraindications) VALUES
(1, N'Проводити натщесерце, за 8 годин до обстеження не вживати їжу.', N'Гострі травми шкіри в зоні сканування.'),
(2, N'Спеціальної підготовки не потрібно.', N'Відсутні.'),
(3, N'Бажано мати попередні результати ЕКГ та виписки.', N'Відсутні.'),
(4, N'Уникати фізичних навантажень за 1 годину до процедури.', N'Наявність кардіостимулятора (потребує уточнення).');

-- Комплексні пакети послуг
INSERT INTO ServicePackages (PackageName, TotalPrice,DiscountPercentage) VALUES
(N'Кардіокомплекс повний', 850.00, 15.00),
(N'Базовий профілактичний', 1100.00, 10.00);

-- Зв'язок M:N з власним полем у PackageServices
INSERT INTO PackageServices (PackageId, ServiceId, Quantity, CustomNotes) VALUES
(1, 3, 1, N'Основна консультація кардіолога в комплексі'),
(1, 4, 1, N'ЕКГ у межах кардіокомплексу'),
(2, 1, 1, N'УЗД обстеження базове');