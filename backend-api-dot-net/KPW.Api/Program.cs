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
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

const string GcpCredentialsFileName = "devson-development-6d4da133b74e.json";

var builder = WebApplication.CreateBuilder(args);

LoadDotEnv(builder.Environment.ContentRootPath);
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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
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
            .AllowCredentials();
    });
    options.AddPolicy("StagingCors", policy =>
    {
        policy.WithOrigins("https://kpw.devson.co.za")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
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
else if (app.Environment.IsStaging())
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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<Microsoft.EntityFrameworkCore.DbContext>();
    try
    {
        await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.MigrateAsync(dbContext.Database);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Automatic EF migration execution encountered an exception, proceeding with raw SQL fallback.");
    }
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
        UPDATE u
        SET u.ClinicId = (SELECT TOP 1 ClinicId FROM Clinics ORDER BY ClinicId ASC)
        FROM Users u
        WHERE u.UserRole = 'Owner' AND u.ClinicId IS NULL AND EXISTS (SELECT 1 FROM Clinics);");

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
}

app.Run();

static void ConfigureGoogleApplicationCredentials(string contentRootPath)
{
    var envPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
    if (!string.IsNullOrWhiteSpace(envPath))
    {
        var fullEnvPath = Path.IsPathRooted(envPath) ? envPath : Path.Combine(contentRootPath, envPath);
        if (File.Exists(fullEnvPath))
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path.GetFullPath(fullEnvPath));
            return;
        }
        else
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", null);
        }
    }

    var credentialsPath = Path.Combine(contentRootPath, GcpCredentialsFileName);
    if (File.Exists(credentialsPath))
    {
        Environment.SetEnvironmentVariable(
            "GOOGLE_APPLICATION_CREDENTIALS",
            Path.GetFullPath(credentialsPath));
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
                }
            }
        }
    }
}
