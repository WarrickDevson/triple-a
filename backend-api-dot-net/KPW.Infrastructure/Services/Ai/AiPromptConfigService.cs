using System.Text.Json;
using KPW.Application.DTOs.Ai;
using KPW.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KPW.Infrastructure.Services.Ai;

public class AiPromptConfigService : IAiPromptConfigService
{
    public const string DefaultSystemPrompt =
        "You are the wellness assistant for Triple A Veterinary Physiotherapy. " +
        "Support pet owners with compassionate, practical, and medically safe guidance regarding pet rehabilitation, recovery expectations, mobility, home exercises, and post-surgery care.\n\n" +
        "Guidelines:\n" +
        "1. Focus directly on the owner's immediate question.\n" +
        "2. NEVER diagnose medical conditions, prescribe medications, or alter dosages. Direct medical concerns to a veterinarian.\n" +
        "3. For worsening pain, severe lameness, or post-surgical red flags, advise prompt evaluation with their vet or Triple A physiotherapist.\n" +
        "4. If asked about appointments or scheduling, guide the owner to the Appointments tab in the Triple A app or clinic reception.\n" +
        "5. Keep responses concise, clear, and structured with bullet points or tables where helpful.\n" +
        "6. When reviewing attached photos or documents, provide observational guidance while noting photos do not replace in-person exams.";

    private readonly string _filePath;
    private readonly ILogger<AiPromptConfigService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private AiPromptConfigDto? _cachedConfig;

    public AiPromptConfigService(IHostEnvironment environment, ILogger<AiPromptConfigService> logger)
    {
        _logger = logger;
        var dir = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "ai_prompt_config.json");
    }

    public async Task<AiPromptConfigDto> GetPromptConfigAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedConfig != null)
        {
            return _cachedConfig;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedConfig != null)
            {
                return _cachedConfig;
            }

            if (File.Exists(_filePath))
            {
                var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
                var stored = JsonSerializer.Deserialize<StoredPromptConfig>(json);
                if (stored != null && !string.IsNullOrWhiteSpace(stored.SystemPrompt))
                {
                    _cachedConfig = new AiPromptConfigDto(
                        SystemPrompt: stored.SystemPrompt,
                        DefaultSystemPrompt: DefaultSystemPrompt,
                        IsCustomized: stored.IsCustomized,
                        LastModifiedAt: stored.LastModifiedAt,
                        LastModifiedBy: stored.LastModifiedBy);
                    return _cachedConfig;
                }
            }

            _cachedConfig = new AiPromptConfigDto(
                SystemPrompt: DefaultSystemPrompt,
                DefaultSystemPrompt: DefaultSystemPrompt,
                IsCustomized: false,
                LastModifiedAt: null,
                LastModifiedBy: null);

            return _cachedConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load AI prompt configuration from {FilePath}, using default prompt.", _filePath);
            return new AiPromptConfigDto(
                SystemPrompt: DefaultSystemPrompt,
                DefaultSystemPrompt: DefaultSystemPrompt,
                IsCustomized: false,
                LastModifiedAt: null,
                LastModifiedBy: null);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<AiPromptConfigDto> UpdatePromptConfigAsync(
        string systemPrompt,
        string? updatedBy,
        CancellationToken cancellationToken = default)
    {
        var trimmed = systemPrompt.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("System prompt cannot be empty.", nameof(systemPrompt));
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var isCustomized = !string.Equals(trimmed, DefaultSystemPrompt.Trim(), StringComparison.Ordinal);
            var now = DateTime.UtcNow;

            var stored = new StoredPromptConfig
            {
                SystemPrompt = trimmed,
                IsCustomized = isCustomized,
                LastModifiedAt = now,
                LastModifiedBy = updatedBy
            };

            var json = JsonSerializer.Serialize(stored, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json, cancellationToken);

            _cachedConfig = new AiPromptConfigDto(
                SystemPrompt: trimmed,
                DefaultSystemPrompt: DefaultSystemPrompt,
                IsCustomized: isCustomized,
                LastModifiedAt: now,
                LastModifiedBy: updatedBy);

            _logger.LogInformation("AI prompt configuration updated by {UpdatedBy}. IsCustomized: {IsCustomized}",
                updatedBy ?? "Unknown", isCustomized);

            return _cachedConfig;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<AiPromptConfigDto> ResetPromptConfigAsync(
        string? updatedBy,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var stored = new StoredPromptConfig
            {
                SystemPrompt = DefaultSystemPrompt,
                IsCustomized = false,
                LastModifiedAt = now,
                LastModifiedBy = updatedBy
            };

            var json = JsonSerializer.Serialize(stored, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json, cancellationToken);

            _cachedConfig = new AiPromptConfigDto(
                SystemPrompt: DefaultSystemPrompt,
                DefaultSystemPrompt: DefaultSystemPrompt,
                IsCustomized: false,
                LastModifiedAt: now,
                LastModifiedBy: updatedBy);

            _logger.LogInformation("AI prompt configuration reset to clinic defaults by {UpdatedBy}.", updatedBy ?? "Unknown");

            return _cachedConfig;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> GetEffectiveSystemPromptAsync(CancellationToken cancellationToken = default)
    {
        var config = await GetPromptConfigAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(config.SystemPrompt) ? DefaultSystemPrompt : config.SystemPrompt;
    }

    private class StoredPromptConfig
    {
        public string SystemPrompt { get; set; } = string.Empty;
        public bool IsCustomized { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
