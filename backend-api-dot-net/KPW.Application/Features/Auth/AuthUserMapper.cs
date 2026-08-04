using KPW.Application.DTOs.Auth;
using KPW.Domain.Entities;

namespace KPW.Application.Features.Auth;

public static class AuthUserMapper
{
    public static AuthUserDto ToDto(User user, Clinic? clinic = null) =>
        new(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.UserRole,
            user.SubscriptionTier,
            user.ClinicId,
            clinic?.ClinicName,
            clinic?.InviteCode);
}
