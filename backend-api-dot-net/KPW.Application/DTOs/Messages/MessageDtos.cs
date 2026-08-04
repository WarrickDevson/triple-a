namespace KPW.Application.DTOs.Messages;

public record MessageDto(
    int MessageId,
    int MessageThreadId,
    int SenderUserId,
    string SenderName,
    string Body,
    int? VideoSubmissionId,
    DateTime? ReadAt,
    DateTime CreatedDate);

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
    int UnreadCount);

public record SendMessageRequestDto(
    string Body,
    int? VideoSubmissionId);
