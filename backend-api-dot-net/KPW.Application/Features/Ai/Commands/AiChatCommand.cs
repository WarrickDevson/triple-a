using KPW.Application.DTOs.Ai;
using KPW.Application.Interfaces;
using MediatR;

namespace KPW.Application.Features.Ai.Commands;

public record AiChatCommand(AiChatRequestDto Request) : IRequest<AiChatResponseDto>;

public class AiChatCommandHandler : IRequestHandler<AiChatCommand, AiChatResponseDto>
{
    private readonly IAiChatService _aiChatService;
    private readonly ICurrentUserService _currentUserService;

    public AiChatCommandHandler(IAiChatService aiChatService, ICurrentUserService currentUserService)
    {
        _aiChatService = aiChatService;
        _currentUserService = currentUserService;
    }

    public async Task<AiChatResponseDto> Handle(AiChatCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException();
        }

        var message = command.Request.Message?.Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidOperationException("Message is required.");
        }

        var result = await _aiChatService.ChatAsync(message, cancellationToken);

        return new AiChatResponseDto(
            result.Answer,
            result.Sources.Select(s => new AiChatSourceDto(s.Title, s.Excerpt)).ToList());
    }
}
