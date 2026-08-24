using KPW.Application;
using KPW.Application.Features.Videos.Commands;
using KPW.Application.Interfaces;
using KPW.Infrastructure.Data;
using KPW.Infrastructure.Services;
using KPW.Infrastructure.Services.Ai;
using KPW.Infrastructure.Services.Reports;
using KPW.Infrastructure.Services.Video;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KPW.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.Configure<VideoOptions>(configuration.GetSection(VideoOptions.SectionName));
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.Configure<AppOptions>(configuration.GetSection(AppOptions.SectionName));
        services.Configure<SendGridOptions>(configuration.GetSection(SendGridOptions.SectionName));

        services.AddSingleton<LoggingEmailSender>();
        services.AddHttpClient<SendGridEmailSender>();
        services.AddTransient<IEmailSender>(sp => sp.GetRequiredService<SendGridEmailSender>());

        RegisterVideoServices(services, configuration);
        RegisterAiServices(services, configuration);

        services.AddSingleton<IVideoProcessingQueue, VideoProcessingQueue>();
        services.AddSingleton<IPetReportPdfGenerator, QuestPetReportPdfGenerator>();
        services.AddSingleton<ISoapReportPdfGenerator, QuestSoapReportPdfGenerator>();
        services.AddHostedService<VideoProcessingBackgroundService>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var rawConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(rawConnectionString))
            {
                rawConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                    ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                    ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
                    ?? Environment.GetEnvironmentVariable("DEPLOY_DB_CONNECTION_STRING");
            }

            var connectionString = rawConnectionString?.Trim().Trim('"', '\'');

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string 'DefaultConnection' is not configured. " +
                    "Please set DB_CONNECTION_STRING or ConnectionStrings__DefaultConnection in .env or via environment variables.");
            }

            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(60);
                });
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static void RegisterVideoServices(IServiceCollection services, IConfiguration configuration)
    {
        var videoProvider = configuration.GetSection(VideoOptions.SectionName).Get<VideoOptions>()?.Provider ?? "Local";
        var gcpCredsEnv = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        var hasGcpCreds = !string.IsNullOrWhiteSpace(gcpCredsEnv) && File.Exists(gcpCredsEnv);

        if (videoProvider.Equals("Google", StringComparison.OrdinalIgnoreCase) && hasGcpCreds)
        {
            services.AddSingleton<GcsVideoStorage>();
            services.AddSingleton<IVideoStorage>(sp => sp.GetRequiredService<GcsVideoStorage>());
            services.AddSingleton<IVideoTranscoder, GoogleVideoTranscoder>();
        }
        else
        {
            services.AddSingleton<LocalVideoStorage>();
            services.AddSingleton<IVideoStorage>(sp => sp.GetRequiredService<LocalVideoStorage>());
            services.AddSingleton<IVideoTranscoder, LocalVideoTranscoder>();
        }
    }

    private static void RegisterAiServices(IServiceCollection services, IConfiguration configuration)
    {
        var aiProvider = configuration.GetSection(AiOptions.SectionName).Get<AiOptions>()?.Provider ?? "Local";

        if (aiProvider.Equals("Vertex", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IAiChatService, VertexAiChatService>();
        }
        else
        {
            services.AddSingleton<IAiChatService, LocalAiChatService>();
        }

        services.AddHttpClient<ISoapVoiceTranscriptionService, SoapVoiceTranscriptionService>();
    }
}

public class AiOptions
{
    public const string SectionName = "Ai";
    public string Provider { get; set; } = "Gemini";
    public string ApiKey { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string Location { get; set; } = "us-central1";
    public string Model { get; set; } = "gemini-2.0-flash";
    public bool UseEducationChunks { get; set; } = false;
}
