namespace KPW.Application.DTOs.Ai;

public record AiChatRequestDto(string Message);

public record AiChatSourceDto(string Title, string Excerpt);

public record AiChatResponseDto(string Answer, IReadOnlyList<AiChatSourceDto> Sources);
