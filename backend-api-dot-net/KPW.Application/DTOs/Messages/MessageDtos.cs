namespace KPW.Application.DTOs.Messages;

public record MessageDto(
    int MessageId,
    int MessageThreadId,
    int SenderUserId,
    string SenderName,
    string Body,
    int? VideoSubmissionId,
    string? VideoTitle,
    string? AttachmentUrl,
    string? AttachmentName,
    string? AttachmentType,
    DateTime? ReadAt,
    DateTime CreatedDate,
    string? SenderProfilePictureUrl = null);

public record MessageThreadDto(
    int MessageThreadId,
    int PetId,
    string PetName,
    int OwnerId,
    string OwnerName,
    int PhysioId,
    string PhysioName,
    string? LastMessagePreview,
    DateTime? LastMessageAt,
    int UnreadCount,
    string? PetProfilePictureUrl = null,
    string? OwnerProfilePictureUrl = null,
    string? PhysioProfilePictureUrl = null);

public record SendMessageRequestDto(
    string Body,
    int? VideoSubmissionId,
    string? AttachmentUrl,
    string? AttachmentName,
    string? AttachmentType);
