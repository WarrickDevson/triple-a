namespace KPW.Application.DTOs.Ai;

public record AiChatMessageTurnDto(string Role, string Content);

public record AiChatRequestDto(
    string Message,
    bool IncludePetContext = false,
    int? PetId = null,
    string? AttachmentUrl = null,
    string? AttachmentBase64 = null,
    string? AttachmentMimeType = null,
    string? AttachmentName = null,
    IReadOnlyList<AiChatMessageTurnDto>? History = null);

public record AiChatSourceDto(
    string Title,
    string Excerpt,
    string? DocumentUrl = null,
    string? DownloadUrl = null);

public record AiChatResponseDto(string Answer, IReadOnlyList<AiChatSourceDto> Sources);

public record AiPromptConfigDto(
    string SystemPrompt,
    string DefaultSystemPrompt,
    bool IsCustomized,
    DateTime? LastModifiedAt,
    string? LastModifiedBy);

public record UpdateAiPromptConfigRequestDto(string SystemPrompt);
