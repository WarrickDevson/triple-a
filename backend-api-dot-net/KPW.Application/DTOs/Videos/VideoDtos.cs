namespace KPW.Application.DTOs.Videos;

public record VideoSubmissionDto(
    int VideoSubmissionId,
    int PetId,
    string PetName,
    int ExerciseId,
    string ExerciseTitle,
    string RawVideoStorageUrl,
    string? ProcessedVideoStreamingUrl,
    string ProcessingStatus,
    bool IsReviewed,
    string? PhysioFeedbackNotes,
    DateTime CreatedDate);

public record ReviewVideoRequestDto(string FeedbackNotes);

public record UploadVideoResultDto(
    int VideoSubmissionId,
    string ProcessingStatus,
    string RawVideoStorageUrl);
