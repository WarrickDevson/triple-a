using KPW.Application.DTOs.Auth;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Auth.Queries;

public record CheckEmailQuery(string? Email) : IRequest<CheckEmailResponseDto>;

public class CheckEmailQueryHandler : IRequestHandler<CheckEmailQuery, CheckEmailResponseDto>
{
    private readonly DbContext _dbContext;

    public CheckEmailQueryHandler(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CheckEmailResponseDto> Handle(CheckEmailQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new CheckEmailResponseDto(false, null);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _dbContext.Set<User>()
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        return new CheckEmailResponseDto(
            exists,
            exists ? "This email address is already registered. Please sign in instead." : null);
    }
}
