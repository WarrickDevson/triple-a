using KPW.Application.DTOs.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;

namespace KPW.Application.Features.Auth;

public static class AuthUserMapper
{
    public static AuthUserDto ToDto(User user, Clinic? clinic = null, IFileStorageService? fileStorage = null) =>
        new(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.UserRole,
            user.SubscriptionTier,
            user.ClinicId,
            clinic?.ClinicName,
            clinic?.InviteCode,
            user.IsEmailVerified,
            user.IsApproved,
            ResolveAvatarUrl(user.ProfilePictureUrl, fileStorage));

    private static string? ResolveAvatarUrl(string? profilePictureUrl, IFileStorageService? fileStorage)
    {
        if (string.IsNullOrWhiteSpace(profilePictureUrl)) return null;

        if (fileStorage != null)
        {
            return fileStorage.GetPermanentUrl(profilePictureUrl);
        }

        if (profilePictureUrl.StartsWith("/api/media", StringComparison.OrdinalIgnoreCase))
        {
            return profilePictureUrl;
        }

        var normalized = profilePictureUrl.TrimStart('/', '\\');
        return $"/api/media/view?path={Uri.EscapeDataString(normalized)}";
    }
}
