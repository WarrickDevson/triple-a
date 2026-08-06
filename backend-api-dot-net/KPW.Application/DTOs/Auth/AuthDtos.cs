namespace KPW.Application.DTOs.Auth;



public record AuthUserDto(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string UserRole,
    string SubscriptionTier,
    int? ClinicId,
    string? ClinicName = null,
    string? ClinicInviteCode = null,
    bool IsEmailVerified = false);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    AuthUserDto User);

public record RegisterRequestDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string InviteCode);

public record LoginRequestDto(
    string Email,
    string Password);

public record RefreshTokenRequestDto(
    string RefreshToken);

public record ForgotPasswordRequestDto(
    string Email);

public record ForgotPasswordResponseDto(
    string Message);

public record ResetPasswordRequestDto(
    string Token,
    string NewPassword);

public record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword);

public record VerifyEmailRequestDto(
    string Email,
    string Token);

public record ResendVerificationEmailRequestDto(
    string Email);

public record SendOwnerInviteRequestDto(
    string RecipientEmail,
    string? OwnerName);

public record MessageResponseDto(
    string Message);


