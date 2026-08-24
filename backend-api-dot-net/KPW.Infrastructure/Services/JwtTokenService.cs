using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KPW.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var rawKey = jwtSettings["Key"]
            ?? Environment.GetEnvironmentVariable("Jwt__Key")
            ?? Environment.GetEnvironmentVariable("JWT_KEY")
            ?? Environment.GetEnvironmentVariable("DEPLOY_JWT_KEY");

        if (string.IsNullOrWhiteSpace(rawKey))
        {
            throw new InvalidOperationException("JWT signing key is not configured. Please set JWT_KEY in .env or via environment variables.");
        }

        var issuer = !string.IsNullOrWhiteSpace(jwtSettings["Issuer"])
            ? jwtSettings["Issuer"]
            : (Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "KPW.MoveWell");

        var audience = !string.IsNullOrWhiteSpace(jwtSettings["Audience"])
            ? jwtSettings["Audience"]
            : (Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "KPW.MoveWell.Clients");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey.Trim().Trim('"', '\'')));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Role, user.UserRole),
            new("subscription_tier", user.SubscriptionTier)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(bytes);
    }
}
