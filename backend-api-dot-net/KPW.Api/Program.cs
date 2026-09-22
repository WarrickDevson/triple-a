using System.Text;
using FluentValidation;
using KPW.Api.Hubs;
using KPW.Api.Services;
using KPW.Application;
using KPW.Application.Interfaces;
using KPW.Application.Features.Videos.Commands;
using KPW.Application.DTOs.Auth;
using KPW.Application.Features.Auth.Commands;
using KPW.Application.Features.Auth.Queries;
using KPW.Infrastructure;
using KPW.Infrastructure.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

const string GcpCredentialsFileName = "devson-development-6d4da133b74e.json";

var builder = WebApplication.CreateBuilder(args);

LoadDotEnv(builder.Environment.ContentRootPath);
builder.Configuration.AddEnvironmentVariables();
ConfigureGoogleApplicationCredentials(builder.Environment.ContentRootPath);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .Filter.ByExcluding(logEvent => false);
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSignalR();
builder.Services.AddScoped<IChatNotificationService, ChatNotificationService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var jwtKey = jwtSettings["Key"]
            ?? Environment.GetEnvironmentVariable("Jwt__Key")
            ?? Environment.GetEnvironmentVariable("JWT_KEY")
            ?? Environment.GetEnvironmentVariable("DEPLOY_JWT_KEY");

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("JWT signing key is not configured. Please set JWT_KEY in .env or via environment variables.");
        }

        var issuer = !string.IsNullOrWhiteSpace(jwtSettings["Issuer"]) ? jwtSettings["Issuer"] : (Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "KPW.MoveWell");
        var audience = !string.IsNullOrWhiteSpace(jwtSettings["Audience"]) ? jwtSettings["Audience"] : (Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "KPW.MoveWell.Clients");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
                {
                    var host = new Uri(origin).Host;
                    return host == "localhost" || host == "127.0.0.1";
                })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition");
    });
    options.AddPolicy("StagingCors", policy =>
    {
        policy.WithOrigins(
                "https://mytriplea.co.za",
                "https://www.mytriplea.co.za")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition");
    });
});

var app = builder.Build();

var pathBase = builder.Configuration["Hosting:PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var maskedIp = PopiaLogEnricher.MaskIpAddresses(httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("ClientIpMasked", maskedIp);
    };
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("KPW MoveWell API");
    });
    app.UseCors("DevCors");
}
else
{
    app.UseCors("StagingCors");
}

var videoOptions = app.Configuration.GetSection(VideoOptions.SectionName).Get<VideoOptions>() ?? new VideoOptions();
var uploadRoot = Path.IsPathRooted(videoOptions.LocalRoot)
    ? videoOptions.LocalRoot
    : Path.Combine(app.Environment.ContentRootPath, videoOptions.LocalRoot);
Directory.CreateDirectory(uploadRoot);
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadRoot),
    RequestPath = "/uploads"
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<Microsoft.EntityFrameworkCore.DbContext>();
    try
    {
        await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(
            dbContext.Database,
            @"IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NOT NULL
              AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProfilePictureUrl')
              AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260910083151_AddProfilePicturesToUserAndPet')
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Pets' AND COLUMN_NAME = 'ProfilePictureUrl')
                BEGIN
                    ALTER TABLE [Pets] ADD [ProfilePictureUrl] nvarchar(1000) NULL;
                END
                INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                VALUES ('20260910083151_AddProfilePicturesToUserAndPet', '8.0.0');
            END");
    }
    catch
    {
        // Non-critical pre-migration alignment check
    }

    try
    {
        await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.MigrateAsync(dbContext.Database);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Automatic EF migration execution encountered an exception, proceeding with raw SQL fallback.");
    }
    try
    {
        await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(
            dbContext.Database,
            @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'AttachmentUrl')
            BEGIN
                ALTER TABLE [Messages] ADD [AttachmentUrl] nvarchar(1000) NULL;
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'AttachmentName')
            BEGIN
                ALTER TABLE [Messages] ADD [AttachmentName] nvarchar(255) NULL;
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'AttachmentType')
            BEGIN
                ALTER TABLE [Messages] ADD [AttachmentType] nvarchar(100) NULL;
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'IsApproved')
            BEGIN
                ALTER TABLE [Users] ADD [IsApproved] bit NOT NULL CONSTRAINT [DF_Users_IsApproved] DEFAULT (1);
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProfilePictureUrl')
            BEGIN
                ALTER TABLE [Users] ADD [ProfilePictureUrl] nvarchar(1000) NULL;
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Pets' AND COLUMN_NAME = 'ProfilePictureUrl')
            BEGIN
                ALTER TABLE [Pets] ADD [ProfilePictureUrl] nvarchar(1000) NULL;
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RehabProgramExercises' AND COLUMN_NAME = 'PhaseId')
            BEGIN
                ALTER TABLE [RehabProgramExercises] ADD [PhaseId] int NOT NULL CONSTRAINT [DF_RehabProgramExercises_PhaseId] DEFAULT (1);
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SoapNotes')
            BEGIN
                CREATE TABLE [SoapNotes] (
                    [SoapNoteId] int NOT NULL IDENTITY,
                    [PetId] int NOT NULL,
                    [PhysioId] int NOT NULL,
                    [AppointmentId] int NULL,
                    [SessionDate] datetime2 NOT NULL,
                    [Subjective] nvarchar(max) NOT NULL,
                    [Objective] nvarchar(max) NOT NULL,
                    [Action] nvarchar(max) NOT NULL,
                    [Plan] nvarchar(max) NOT NULL,
                    [StiffnessScore] int NULL,
                    [PainScore] int NULL,
                    [LamenessScore] int NULL,
                    [CustomMetricsJson] nvarchar(max) NULL,
                    [IsSharedWithOwner] bit NOT NULL DEFAULT CAST(0 AS bit),
                    [SharedAtUtc] datetime2 NULL,
                    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
                    [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                    [CreatedUserId] int NULL,
                    [ModifiedDate] datetime2 NULL,
                    [ModifiedUserId] int NULL,
                    CONSTRAINT [PK_SoapNotes] PRIMARY KEY ([SoapNoteId]),
                    CONSTRAINT [FK_SoapNotes_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([PetId]) ON DELETE CASCADE,
                    CONSTRAINT [FK_SoapNotes_Users_PhysioId] FOREIGN KEY ([PhysioId]) REFERENCES [Users] ([UserId]),
                    CONSTRAINT [FK_SoapNotes_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId])
                );
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SharedReports')
            BEGIN
                CREATE TABLE [SharedReports] (
                    [SharedReportId] int NOT NULL IDENTITY,
                    [PetId] int NOT NULL,
                    [SoapNoteId] int NULL,
                    [SharedByPhysioId] int NOT NULL,
                    [Title] nvarchar(200) NOT NULL,
                    [ReportType] nvarchar(50) NOT NULL,
                    [Summary] nvarchar(2000) NULL,
                    [FileUrl] nvarchar(1000) NULL,
                    [SharedAtUtc] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
                    [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                    [CreatedUserId] int NULL,
                    [ModifiedDate] datetime2 NULL,
                    [ModifiedUserId] int NULL,
                    CONSTRAINT [PK_SharedReports] PRIMARY KEY ([SharedReportId]),
                    CONSTRAINT [FK_SharedReports_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([PetId]) ON DELETE CASCADE,
                    CONSTRAINT [FK_SharedReports_SoapNotes_SoapNoteId] FOREIGN KEY ([SoapNoteId]) REFERENCES [SoapNotes] ([SoapNoteId]),
                    CONSTRAINT [FK_SharedReports_Users_SharedByPhysioId] FOREIGN KEY ([SharedByPhysioId]) REFERENCES [Users] ([UserId])
                );
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SharedReports' AND COLUMN_NAME = 'FileUrl')
            BEGIN
                ALTER TABLE [SharedReports] ADD [FileUrl] nvarchar(1000) NULL;
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OwnerSubjectiveNotes')
            BEGIN
                CREATE TABLE [OwnerSubjectiveNotes] (
                    [OwnerSubjectiveNoteId] int NOT NULL IDENTITY,
                    [PetId] int NOT NULL,
                    [OwnerId] int NOT NULL,
                    [NoteDate] datetime2 NOT NULL,
                    [Notes] nvarchar(2000) NOT NULL,
                    [PainObserved] int NULL,
                    [EnergyObserved] int NULL,
                    [IsReviewed] bit NOT NULL,
                    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
                    [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                    [CreatedUserId] int NULL,
                    [ModifiedDate] datetime2 NULL,
                    [ModifiedUserId] int NULL,
                    CONSTRAINT [PK_OwnerSubjectiveNotes] PRIMARY KEY ([OwnerSubjectiveNoteId]),
                    CONSTRAINT [FK_OwnerSubjectiveNotes_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([PetId]) ON DELETE CASCADE,
                    CONSTRAINT [FK_OwnerSubjectiveNotes_Users_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([UserId])
                );
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Exercises' AND COLUMN_NAME = 'CoverImageUrl')
            BEGIN
                ALTER TABLE [Exercises] ADD [CoverImageUrl] nvarchar(2048) NULL;
            END
            ELSE IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Exercises' AND COLUMN_NAME = 'CoverImageUrl' AND (CHARACTER_MAXIMUM_LENGTH < 2048 AND CHARACTER_MAXIMUM_LENGTH > 0))
            BEGIN
                ALTER TABLE [Exercises] ALTER COLUMN [CoverImageUrl] nvarchar(2048) NULL;
            END
            IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Exercises' AND COLUMN_NAME = 'VideoUrl' AND (CHARACTER_MAXIMUM_LENGTH < 2048 AND CHARACTER_MAXIMUM_LENGTH > 0))
            BEGIN
                ALTER TABLE [Exercises] ALTER COLUMN [VideoUrl] nvarchar(2048) NULL;
            END
            IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ExerciseSteps' AND COLUMN_NAME = 'ImageUrl' AND (CHARACTER_MAXIMUM_LENGTH < 2048 AND CHARACTER_MAXIMUM_LENGTH > 0))
            BEGIN
                ALTER TABLE [ExerciseSteps] ALTER COLUMN [ImageUrl] nvarchar(2048) NULL;
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Exercises' AND COLUMN_NAME = 'IsSystemDefault')
            BEGIN
                ALTER TABLE [Exercises] ADD [IsSystemDefault] bit NOT NULL CONSTRAINT [DF_Exercises_IsSystemDefault] DEFAULT (1);
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Exercises' AND COLUMN_NAME = 'ClinicId')
            BEGIN
                ALTER TABLE [Exercises] ADD [ClinicId] int NULL;
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Exercises' AND COLUMN_NAME = 'BaseExerciseId')
            BEGIN
                ALTER TABLE [Exercises] ADD [BaseExerciseId] int NULL;
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Exercises' AND COLUMN_NAME = 'IsActiveForOwners')
            BEGIN
                ALTER TABLE [Exercises] ADD [IsActiveForOwners] bit NOT NULL CONSTRAINT [DF_Exercises_IsActiveForOwners] DEFAULT (1);
            END
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Exercises' AND COLUMN_NAME = 'VideoVariationsJson')
            BEGIN
                ALTER TABLE [Exercises] ADD [VideoVariationsJson] nvarchar(max) NULL;
            END");

        await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(
            dbContext.Database,
            @"UPDATE u
            SET u.ClinicId = (SELECT TOP 1 ClinicId FROM Clinics ORDER BY ClinicId ASC)
            FROM Users u
            WHERE u.UserRole = 'Owner' AND u.ClinicId IS NULL AND EXISTS (SELECT 1 FROM Clinics);
            UPDATE [Exercises] SET [VideoUrl] = 'https://www.youtube.com/watch?v=FBy2E1f10WM' WHERE [ExerciseId] = 1 AND ([VideoUrl] LIKE '%sample%' OR [VideoUrl] LIKE '%ForBiggerBlazes%');
            UPDATE [Exercises] SET [VideoUrl] = 'https://www.youtube.com/watch?v=Z8UYivdkyhM' WHERE [ExerciseId] = 2 AND ([VideoUrl] LIKE '%sample%' OR [VideoUrl] LIKE '%ForBiggerBlazes%');
            UPDATE [Exercises] SET [VideoUrl] = 'https://www.youtube.com/watch?v=aC_QNY5t8n8' WHERE [ExerciseId] = 3 AND ([VideoUrl] LIKE '%sample%' OR [VideoUrl] LIKE '%ForBiggerBlazes%');
            UPDATE [Exercises] SET [VideoUrl] = 'https://www.youtube.com/watch?v=TRDnqYOtlKM' WHERE [ExerciseId] = 4 AND ([VideoUrl] LIKE '%sample%' OR [VideoUrl] LIKE '%ForBiggerBlazes%');
            UPDATE [Exercises] SET [VideoUrl] = 'https://www.youtube.com/watch?v=1byv8TzSEbU' WHERE [ExerciseId] = 5 AND ([VideoUrl] LIKE '%sample%' OR [VideoUrl] LIKE '%ForBiggerBlazes%');
            UPDATE [Exercises] SET [VideoUrl] = 'https://www.youtube.com/watch?v=h-IQU8mPqZM' WHERE [ExerciseId] = 6 AND ([VideoUrl] LIKE '%sample%' OR [VideoUrl] LIKE '%ForBiggerBlazes%');
            UPDATE [Exercises] SET [VideoUrl] = 'https://www.youtube.com/watch?v=-XRBJ7oPw74' WHERE [ExerciseId] = 7 AND ([VideoUrl] LIKE '%sample%' OR [VideoUrl] LIKE '%ForBiggerBlazes%');
            UPDATE [Exercises] SET [VideoUrl] = 'https://www.youtube.com/watch?v=FeoKoM7D5SI' WHERE [ExerciseId] = 8 AND ([VideoUrl] LIKE '%sample%' OR [VideoUrl] LIKE '%ForBiggerBlazes%');
            
            IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Exercises' AND COLUMN_NAME = 'VideoVariationsJson')
            BEGIN
                EXEC(N'UPDATE [Exercises] SET [VideoVariationsJson] = ''[{{{{""""Species"""":""""Canine"""",""""BreedCategory"""":""""Chondrodystrophic (Dachshund/Corgi/Basset)"""",""""VideoUrl"""":""""https://www.youtube.com/watch?v=TRDnqYOtlKM"""",""""Title"""":""""Cavaletti for Low-Rider / Long-Backed Dogs"""",""""Notes"""":""""Poles set at wrist/hock height (2-3 inches max) with 1.5x body length spacing.""""}}}},{{{{""""Species"""":""""Canine"""",""""BreedCategory"""":""""Large / Giant Breeds"""",""""VideoUrl"""":""""https://www.youtube.com/watch?v=TRDnqYOtlKM"""",""""Title"""":""""Cavaletti for Large Dogs"""",""""Notes"""":""""Poles spaced at standard shoulder-height stride distance to promote full extension.""""}}}} ]'' WHERE [ExerciseId] = 4 AND [VideoVariationsJson] IS NULL');
                EXEC(N'UPDATE [Exercises] SET [VideoVariationsJson] = ''[{{{{""""Species"""":""""Feline"""",""""BreedCategory"""":""""All Cats"""",""""VideoUrl"""":""""https://www.youtube.com/watch?v=-XRBJ7oPw74"""",""""Title"""":""""Feline Passive Range of Motion"""",""""Notes"""":""""Gentle low-stress handling with towel wrap; small amplitude flexion/extension.""""}}}} ]'' WHERE [ExerciseId] = 2 AND [VideoVariationsJson] IS NULL');
            END");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Schema bootstrap encountered a non-critical exception. Tables already initialized or database is busy.");
    }

    try
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<KPW.Application.Interfaces.IPasswordHasher>();
        var seedUsers = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.IgnoreQueryFilters(
                dbContext.Set<KPW.Domain.Entities.User>())
                .Where(u => u.Email == "physio@kpw.local" || u.Email == "owner@kpw.local" || u.Email == "sysadmin@kpw.local"));

        bool updated = false;
        foreach (var user in seedUsers)
        {
            if (!user.IsActive || !user.IsEmailVerified || !user.IsApproved)
            {
                user.IsActive = true;
                user.IsEmailVerified = true;
                user.IsApproved = true;
                updated = true;
            }

            if (string.IsNullOrWhiteSpace(user.PasswordHash) || !passwordHasher.VerifyPassword("ChangeMe!123", user.PasswordHash))
            {
                user.PasswordHash = passwordHasher.HashPassword("ChangeMe!123");
                updated = true;
            }
        }
        if (updated)
        {
            await dbContext.SaveChangesAsync();
        }

        var fileStorage = scope.ServiceProvider.GetService<IFileStorageService>();
        if (fileStorage != null)
        {
            await SanitizeStorageUrlsAsync(dbContext, fileStorage);
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Seed user verification or storage sanitization encountered a transient exception during startup.");
    }
}

app.Run();

static async Task SanitizeStorageUrlsAsync(DbContext dbContext, IFileStorageService fileStorage)
{
    try
    {
        bool anyChanged = false;

        // 1. Sanitize Users.ProfilePictureUrl
        var usersWithSignedUrls = await dbContext.Set<KPW.Domain.Entities.User>()
            .IgnoreQueryFilters()
            .Where(u => u.ProfilePictureUrl != null && (u.ProfilePictureUrl.Contains("Expires=") || u.ProfilePictureUrl.Contains("storage.googleapis.com")))
            .ToListAsync();

        foreach (var u in usersWithSignedUrls)
        {
            var normalized = fileStorage.NormalizeStoragePath(u.ProfilePictureUrl);
            if (!string.IsNullOrWhiteSpace(normalized) && normalized != u.ProfilePictureUrl)
            {
                u.ProfilePictureUrl = normalized;
                anyChanged = true;
            }
        }

        // 2. Sanitize Pets.ProfilePictureUrl
        var petsWithSignedUrls = await dbContext.Set<KPW.Domain.Entities.Pet>()
            .IgnoreQueryFilters()
            .Where(p => p.ProfilePictureUrl != null && (p.ProfilePictureUrl.Contains("Expires=") || p.ProfilePictureUrl.Contains("storage.googleapis.com")))
            .ToListAsync();

        foreach (var p in petsWithSignedUrls)
        {
            var normalized = fileStorage.NormalizeStoragePath(p.ProfilePictureUrl);
            if (!string.IsNullOrWhiteSpace(normalized) && normalized != p.ProfilePictureUrl)
            {
                p.ProfilePictureUrl = normalized;
                anyChanged = true;
            }
        }

        // 3. Sanitize SharedReports.FileUrl
        var reportsWithSignedUrls = await dbContext.Set<KPW.Domain.Entities.SharedReport>()
            .IgnoreQueryFilters()
            .Where(r => r.FileUrl != null && (r.FileUrl.Contains("Expires=") || r.FileUrl.Contains("storage.googleapis.com")))
            .ToListAsync();

        foreach (var r in reportsWithSignedUrls)
        {
            var normalized = fileStorage.NormalizeStoragePath(r.FileUrl);
            if (!string.IsNullOrWhiteSpace(normalized) && normalized != r.FileUrl)
            {
                r.FileUrl = normalized;
                anyChanged = true;
            }
        }

        // 4. Sanitize Messages.AttachmentUrl
        var messagesWithSignedUrls = await dbContext.Set<KPW.Domain.Entities.Message>()
            .IgnoreQueryFilters()
            .Where(m => m.AttachmentUrl != null && (m.AttachmentUrl.Contains("Expires=") || m.AttachmentUrl.Contains("storage.googleapis.com")))
            .ToListAsync();

        foreach (var m in messagesWithSignedUrls)
        {
            var normalized = fileStorage.NormalizeStoragePath(m.AttachmentUrl);
            if (!string.IsNullOrWhiteSpace(normalized) && normalized != m.AttachmentUrl)
            {
                m.AttachmentUrl = normalized;
                anyChanged = true;
            }
        }

        // 5. Sanitize Exercises.CoverImageUrl & VideoUrl
        var exercisesWithSignedUrls = await dbContext.Set<KPW.Domain.Entities.Exercise>()
            .IgnoreQueryFilters()
            .Where(e => (e.CoverImageUrl != null && (e.CoverImageUrl.Contains("Expires=") || e.CoverImageUrl.Contains("storage.googleapis.com"))) ||
                        (e.VideoUrl != null && (e.VideoUrl.Contains("Expires=") || e.VideoUrl.Contains("storage.googleapis.com"))))
            .ToListAsync();

        foreach (var e in exercisesWithSignedUrls)
        {
            if (e.CoverImageUrl != null && (e.CoverImageUrl.Contains("Expires=") || e.CoverImageUrl.Contains("storage.googleapis.com")))
            {
                var normalized = fileStorage.NormalizeStoragePath(e.CoverImageUrl);
                if (!string.IsNullOrWhiteSpace(normalized) && normalized != e.CoverImageUrl)
                {
                    e.CoverImageUrl = normalized;
                    anyChanged = true;
                }
            }

            if (e.VideoUrl != null && (e.VideoUrl.Contains("Expires=") || e.VideoUrl.Contains("storage.googleapis.com")))
            {
                var normalized = fileStorage.NormalizeStoragePath(e.VideoUrl);
                if (!string.IsNullOrWhiteSpace(normalized) && normalized != e.VideoUrl)
                {
                    e.VideoUrl = normalized;
                    anyChanged = true;
                }
            }
        }

        if (anyChanged)
        {
            await dbContext.SaveChangesAsync();
            Log.Information("Successfully sanitized existing expiring storage URLs into canonical relative paths.");
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to sanitize existing storage URLs during startup.");
    }
}

static void ConfigureGoogleApplicationCredentials(string contentRootPath)
{
    var envPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
    if (!string.IsNullOrWhiteSpace(envPath))
    {
        var candidates = new[]
        {
            envPath,
            Path.IsPathRooted(envPath) ? envPath : Path.Combine(contentRootPath, envPath),
            Path.IsPathRooted(envPath) ? envPath : Path.Combine(contentRootPath, "..", "..", envPath)
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path.GetFullPath(candidate));
                return;
            }
        }

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", null);
    }

    var defaultCandidates = new[]
    {
        Path.Combine(contentRootPath, GcpCredentialsFileName),
        Path.Combine(contentRootPath, "..", "..", GcpCredentialsFileName)
    };

    foreach (var candidate in defaultCandidates)
    {
        if (File.Exists(candidate))
        {
            Environment.SetEnvironmentVariable(
                "GOOGLE_APPLICATION_CREDENTIALS",
                Path.GetFullPath(candidate));
            return;
        }
    }
}

static void LoadDotEnv(string contentRootPath)
{
    var candidates = new[]
    {
        Path.Combine(contentRootPath, ".env"),
        Path.Combine(contentRootPath, "..", ".env"),
        Path.Combine(contentRootPath, "..", "..", ".env")
    };

    foreach (var file in candidates)
    {
        if (!File.Exists(file)) continue;

        foreach (var rawLine in File.ReadAllLines(file))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim().Trim('"', '\'');
                if (!string.IsNullOrWhiteSpace(key))
                {
                    Environment.SetEnvironmentVariable(key, value);

                    // Map common alias names to standard ASP.NET Core configuration keys
                    if (key.Equals("DB_CONNECTION_STRING", StringComparison.OrdinalIgnoreCase) ||
                        key.Equals("DEFAULT_CONNECTION", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", value);
                    }
                    else if (key.Equals("JWT_KEY", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("Jwt__Key", value);
                    }
                    else if (key.Equals("JWT_ISSUER", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("Jwt__Issuer", value);
                    }
                    else if (key.Equals("JWT_AUDIENCE", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("Jwt__Audience", value);
                    }
                    else if (key.Equals("SENDGRID_API_KEY", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("SendGrid__ApiKey", value);
                    }
                    else if (key.Equals("SENDGRID_PROVIDER", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("SendGrid__Provider", value);
                    }
                    else if (key.Equals("SENDGRID_FROM_EMAIL", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("SendGrid__FromEmail", value);
                    }
                    else if (key.Equals("SENDGRID_FROM_NAME", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("SendGrid__FromName", value);
                    }
                    else if (key.Equals("AI_API_KEY", StringComparison.OrdinalIgnoreCase) ||
                             key.Equals("GEMINI_API_KEY", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("Ai__ApiKey", value);
                    }
                    else if (key.Equals("AI_PROVIDER", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("Ai__Provider", value);
                    }
                    else if (key.Equals("AI_MODEL", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("Ai__Model", value);
                    }
                    else if (key.Equals("AI_PROJECT_ID", StringComparison.OrdinalIgnoreCase) ||
                             key.Equals("GCP_PROJECT_ID", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("Ai__ProjectId", value);
                    }
                    else if (key.Equals("AI_LOCATION", StringComparison.OrdinalIgnoreCase) ||
                             key.Equals("GCP_LOCATION", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("Ai__Location", value);
                    }
                }
            }
        }
    }
}
