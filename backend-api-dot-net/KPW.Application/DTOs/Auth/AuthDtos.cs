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
    bool IsEmailVerified = false,
    bool IsApproved = true);

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
    string? PhoneNumber = null,
    string? InviteCode = null,
    string? Role = null,
    string? ClinicName = null);

public record PhysioApprovalDto(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string UserRole,
    int? ClinicId,
    string? ClinicName,
    bool IsEmailVerified,
    bool IsApproved,
    bool IsActive,
    DateTime CreatedDate);

public record SendAdminInviteRequestDto(
    string RecipientEmail,
    string? ClinicName = null);

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

public record VerifyEmailResponseDto(
    string Message,
    bool IsEmailVerified,
    bool IsApproved,
    string UserRole);

public record CheckEmailResponseDto(
    bool Exists,
    string? Message = null);

public record ResendVerificationEmailRequestDto(
    string Email);

public record SendOwnerInviteRequestDto(
    string RecipientEmail,
    string? OwnerName);

public record MessageResponseDto(
    string Message);

public record UpdateProfileRequestDto(
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? ClinicName = null);

public record DataDeletionRequestDto(
    string Email,
    string? RequestType = null,
    string? Reason = null,
    string? AdditionalNotes = null);

public record DataDeletionResponseDto(
    bool Success,
    string Message,
    string RequestReference);

public record AdminUserSummaryDto(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string UserRole,
    int? ClinicId,
    string? ClinicName,
    bool IsActive,
    bool IsApproved,
    int PetCount,
    DateTime CreatedDate,
    bool IsEmailVerified = false);

public record AdminPurgeUserRequestDto(
    bool PurgeMediaAndLogs = true,
    string? AdminNotes = null);



