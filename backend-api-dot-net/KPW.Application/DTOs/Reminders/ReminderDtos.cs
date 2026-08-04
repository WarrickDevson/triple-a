namespace KPW.Application.DTOs.Reminders;

public record ReminderDto(
    string Type,
    string Title,
    string Body,
    int PetId,
    string PetName,
    DateTime? DueAt,
    int? RelatedId);
