IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Addrees] (
    [Id] uniqueidentifier NOT NULL,
    [Street] nvarchar(max) NOT NULL,
    [City] nvarchar(max) NOT NULL,
    [Number] int NOT NULL,
    CONSTRAINT [PK_Addrees] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [PortFoiloItems] (
    [Id] uniqueidentifier NOT NULL DEFAULT (NEWID()),
    [ProjectName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_PortFoiloItems] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Owners] (
    [Id] uniqueidentifier NOT NULL DEFAULT (NEWID()),
    [FullName] nvarchar(max) NOT NULL,
    [Avatar] nvarchar(max) NOT NULL,
    [Profile] nvarchar(max) NOT NULL,
    [AddressId] uniqueidentifier NULL,
    CONSTRAINT [PK_Owners] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Owners_Addrees_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Addrees] ([Id])
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AddressId', N'Avatar', N'FullName', N'Profile') AND [object_id] = OBJECT_ID(N'[Owners]'))
    SET IDENTITY_INSERT [Owners] ON;
INSERT INTO [Owners] ([Id], [AddressId], [Avatar], [FullName], [Profile])
VALUES ('b2dd39b7-104a-4521-b170-08c15a350831', NULL, N'avatar.jpg', N'Ali-Hammad', N'ASP.NET APP Training');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AddressId', N'Avatar', N'FullName', N'Profile') AND [object_id] = OBJECT_ID(N'[Owners]'))
    SET IDENTITY_INSERT [Owners] OFF;
GO

CREATE INDEX [IX_Owners_AddressId] ON [Owners] ([AddressId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118130808_InitialMig6565', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [Owners];
GO

DROP TABLE [PortFoiloItems];
GO

DROP TABLE [Addrees];
GO

CREATE TABLE [Cars] (
    [Id] int NOT NULL IDENTITY,
    [PlateNumber] nvarchar(20) NOT NULL,
    [Model] nvarchar(50) NOT NULL,
    [Year] int NOT NULL,
    [PricePerDay] decimal(10,2) NOT NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_Cars] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(100) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [DrivingLicenseNumber] nvarchar(50) NOT NULL,
    [LicenseExpiryDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [RentalContracts] (
    [Id] int NOT NULL IDENTITY,
    [CarId] int NOT NULL,
    [CustomerId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [TotalPrice] decimal(10,2) NOT NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_RentalContracts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RentalContracts_Cars_CarId] FOREIGN KEY ([CarId]) REFERENCES [Cars] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RentalContracts_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Payments] (
    [Id] int NOT NULL IDENTITY,
    [RentalContractId] int NOT NULL,
    [Amount] decimal(10,2) NOT NULL,
    [PaymentDate] datetime2 NOT NULL,
    [Method] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_RentalContracts_RentalContractId] FOREIGN KEY ([RentalContractId]) REFERENCES [RentalContracts] ([Id]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_Cars_PlateNumber] ON [Cars] ([PlateNumber]);
GO

CREATE INDEX [IX_Payments_RentalContractId] ON [Payments] ([RentalContractId]);
GO

CREATE INDEX [IX_RentalContracts_CarId] ON [RentalContracts] ([CarId]);
GO

CREATE INDEX [IX_RentalContracts_CustomerId] ON [RentalContracts] ([CustomerId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118140814_InitialMig6', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [RentalContracts] ADD [ActualEndDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

ALTER TABLE [RentalContracts] ADD [DailyPrice] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [RentalContracts] ADD [Notes] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [RentalContracts] ADD [TotalAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260119162930_InitialMig', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Cars_PlateNumber] ON [Cars];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RentalContracts]') AND [c].[name] = N'TotalPrice');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [RentalContracts] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [RentalContracts] DROP COLUMN [TotalPrice];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'FullName');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Customers] DROP COLUMN [FullName];
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RentalContracts]') AND [c].[name] = N'ActualEndDate');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [RentalContracts] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [RentalContracts] ALTER COLUMN [ActualEndDate] datetime2 NULL;
GO

ALTER TABLE [RentalContracts] ADD [ExtraFees] decimal(18,2) NULL;
GO

ALTER TABLE [RentalContracts] ADD [FinalAmount] decimal(18,2) NULL;
GO

ALTER TABLE [RentalContracts] ADD [PaidAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [RentalContracts] ADD [PaymentStatus] int NOT NULL DEFAULT 0;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'Method');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Payments] ALTER COLUMN [Method] int NOT NULL;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'Amount');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Payments] ALTER COLUMN [Amount] decimal(18,2) NOT NULL;
GO

ALTER TABLE [Payments] ADD [Purpose] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Payments] ADD [Status] int NOT NULL DEFAULT 0;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Phone');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Customers] ALTER COLUMN [Phone] nvarchar(max) NOT NULL;
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'DrivingLicenseNumber');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Customers] ALTER COLUMN [DrivingLicenseNumber] nvarchar(max) NOT NULL;
GO

ALTER TABLE [Customers] ADD [Email] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Customers] ADD [Name] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Customers] ADD [PasswordHash] nvarchar(max) NOT NULL DEFAULT N'';
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cars]') AND [c].[name] = N'PricePerDay');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Cars] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Cars] ALTER COLUMN [PricePerDay] decimal(18,2) NOT NULL;
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cars]') AND [c].[name] = N'PlateNumber');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Cars] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [Cars] ALTER COLUMN [PlateNumber] nvarchar(max) NOT NULL;
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cars]') AND [c].[name] = N'Model');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Cars] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Cars] ALTER COLUMN [Model] nvarchar(max) NOT NULL;
GO

ALTER TABLE [Cars] ADD [ImageUrl] varbinary(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260125171942_InitialCreatedMig', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cars]') AND [c].[name] = N'ImageUrl');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Cars] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [Cars] ALTER COLUMN [ImageUrl] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260127143653_InitialNewADDcar', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cars]') AND [c].[name] = N'ImageUrl');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Cars] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Cars] ALTER COLUMN [ImageUrl] nvarchar(max) NULL;
GO

CREATE TABLE [Employee] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [Role] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Employee] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260129105246_AddedEmployeesWithRoles', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [RentalContracts] DROP CONSTRAINT [FK_RentalContracts_Customers_CustomerId];
GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RentalContracts]') AND [c].[name] = N'CustomerId');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [RentalContracts] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [RentalContracts] ALTER COLUMN [CustomerId] int NULL;
GO

ALTER TABLE [RentalContracts] ADD [EmployeeId] int NOT NULL DEFAULT 0;
GO

CREATE INDEX [IX_RentalContracts_EmployeeId] ON [RentalContracts] ([EmployeeId]);
GO

ALTER TABLE [RentalContracts] ADD CONSTRAINT [FK_RentalContracts_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]);
GO

ALTER TABLE [RentalContracts] ADD CONSTRAINT [FK_RentalContracts_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employee] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260129195918_RemoveCustomerFromRental', N'8.0.23');
GO

COMMIT;
GO

