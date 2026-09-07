using KPW.Application.DTOs.Ai;

namespace KPW.Application.Interfaces;

public interface IAiPromptConfigService
{
    Task<AiPromptConfigDto> GetPromptConfigAsync(CancellationToken cancellationToken = default);
    Task<AiPromptConfigDto> UpdatePromptConfigAsync(string systemPrompt, string? updatedBy, CancellationToken cancellationToken = default);
    Task<AiPromptConfigDto> ResetPromptConfigAsync(string? updatedBy, CancellationToken cancellationToken = default);
    Task<string> GetEffectiveSystemPromptAsync(CancellationToken cancellationToken = default);
}
