using System.Text;
using FluentValidation;
using KPW.Application;
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
        policy.WithOrigins(
                "http://localhost:5287",
                "http://localhost:5173",
                "http://localhost:3000",
                "http://localhost:8068")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
    options.AddPolicy("StagingCors", policy =>
    {
        policy.WithOrigins(
                "https://www.mytriplea.co.za",
                "https://app.mytriple.co.za",
                "https://owner.mytriplea.co.za")
            .AllowAnyHeader()
            .AllowAnyMethod();
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
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadRoot),
    RequestPath = "/uploads"
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapControllers();

app.Run();

static void ConfigureGoogleApplicationCredentials(string contentRootPath)
{
    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")))
    {
        return;
    }

    var credentialsPath = Path.Combine(contentRootPath, GcpCredentialsFileName);
    if (!File.Exists(credentialsPath))
    {
        return;
    }

    Environment.SetEnvironmentVariable(
        "GOOGLE_APPLICATION_CREDENTIALS",
        Path.GetFullPath(credentialsPath));
}
