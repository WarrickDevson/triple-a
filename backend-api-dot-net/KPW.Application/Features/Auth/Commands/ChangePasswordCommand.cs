using KPW.Application.DTOs.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Auth.Commands;

public record ChangePasswordCommand(ChangePasswordRequestDto Request) : IRequest<MessageResponseDto>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, MessageResponseDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }

    public async Task<MessageResponseDto> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.UserId == _currentUserService.UserId, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        if (!_passwordHasher.VerifyPassword(command.Request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(command.Request.NewPassword);
        user.RefreshTokenHash = null;
        user.RefreshTokenExpiresAt = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MessageResponseDto("Your password has been updated.");
    }
}
