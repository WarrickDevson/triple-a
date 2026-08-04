/*
  KPW MoveWell — SQL Server login + database user
  Server: sql.devson.co.za
  Database: KPW_MoveWell (shared Dev / QA)

  Run as sysadmin on sql.devson.co.za (SSMS, Azure Data Studio, or sqlcmd).

  BEFORE RUNNING:
  1. Replace KPW_MoveWell_DevQa_S3cret! below with your password (all 3 occurrences).
  2. Use the SAME password in appsettings.json, appsettings.Development.json,
     and appsettings.Staging.json.

  AFTER RUNNING:
    dotnet ef database update --project KPW.Infrastructure --startup-project KPW.Api
*/

USE [master];
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'kpw_app')
BEGIN
    CREATE LOGIN [kpw_app]
        WITH PASSWORD = N'KPW_MoveWell_DevQa_S3cret!',
        CHECK_POLICY = ON,
        CHECK_EXPIRATION = OFF;
    PRINT 'Created server login: kpw_app';
END
ELSE
BEGIN
    ALTER LOGIN [kpw_app]
        WITH PASSWORD = N'KPW_MoveWell_DevQa_S3cret!',
        CHECK_POLICY = ON,
        CHECK_EXPIRATION = OFF;
    PRINT 'Login kpw_app already exists; password updated.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'KPW_MoveWell')
BEGIN
    CREATE DATABASE [KPW_MoveWell];
    PRINT 'Created database: KPW_MoveWell';
END
ELSE
    PRINT 'Database KPW_MoveWell already exists.';
GO

USE [KPW_MoveWell];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'kpw_app')
BEGIN
    CREATE USER [kpw_app] FOR LOGIN [kpw_app];
    PRINT 'Created database user: kpw_app';
END
ELSE
    PRINT 'Database user kpw_app already exists.';
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    INNER JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
    INNER JOIN sys.database_principals m ON drm.member_principal_id = m.principal_id
    WHERE r.name = N'db_owner' AND m.name = N'kpw_app'
)
BEGIN
    ALTER ROLE [db_owner] ADD MEMBER [kpw_app];
    PRINT 'Granted db_owner to kpw_app.';
END
GO

PRINT 'Connection string:';
PRINT 'Server=sql.devson.co.za;Database=KPW_MoveWell;User Id=kpw_app;Password=<your password>;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true';
GO
