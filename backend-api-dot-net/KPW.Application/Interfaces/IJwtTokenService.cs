using KPW.Domain.Entities;

namespace KPW.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
