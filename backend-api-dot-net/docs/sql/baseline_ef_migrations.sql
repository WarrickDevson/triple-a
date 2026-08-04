/*
  Baseline EF migration history for an existing KPW_MoveWell database.

  Use when tables already exist but __EFMigrationsHistory is empty (or missing
  rows), and "dotnet ef database update" fails with:
    "There is already an object named 'Clinics' in the database."

  Prerequisites — verify schema is current BEFORE baselining:
    SELECT OBJECT_ID(N'MessageThreads');      -- must NOT be NULL
    SELECT OBJECT_ID(N'ExerciseSessionLogs'); -- must NOT be NULL

  If those tables are missing, do NOT run this script. Instead apply migrations
  normally on a fresh database, or run the missing schema migrations manually.

  Steps:
    1. Run the verification queries above on KPW_MoveWell.
    2. Run this script (SSMS or sqlcmd against sql.devson.co.za).
    3. From backend-api-dot-net:
         dotnet ef database update --project KPW.Infrastructure --startup-project KPW.Api
       This applies only pending migrations (e.g. ExpandStagingSeedData).
*/

USE [KPW_MoveWell];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

-- Skip migrations already recorded.
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
SELECT v.MigrationId, v.ProductVersion
FROM (VALUES
    (N'20260723185226_InitialCreate',                         N'9.0.4'),
    (N'20260723192011_AddExerciseFiltersAndDemoProgram',       N'9.0.4'),
    (N'20260723192607_Phase4DashboardSeed',                   N'9.0.4'),
    (N'20260724082727_Phase5VideoProcessingStatus',           N'9.0.4'),
    (N'20260727133708_PhaseA5A6MessagingReminders',         N'9.0.4')
) AS v(MigrationId, ProductVersion)
WHERE NOT EXISTS (
    SELECT 1
    FROM [__EFMigrationsHistory] h
    WHERE h.MigrationId = v.MigrationId
);
GO

SELECT [MigrationId], [ProductVersion]
FROM [__EFMigrationsHistory]
ORDER BY [MigrationId];
GO
