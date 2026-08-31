using KPW.Domain.Common;
using KPW.Domain.Enums;

namespace KPW.Domain.Entities;

public class VideoSubmission : AuditableEntity
{
    public int VideoSubmissionId { get; set; }
    public int PetId { get; set; }
    public int? ExerciseId { get; set; }
    public string? Title { get; set; }
    public string? Notes { get; set; }
    public string RawVideoStorageUrl { get; set; } = string.Empty;
    public string? ProcessedVideoStreamingUrl { get; set; }
    public string? PhysioFeedbackNotes { get; set; }
    public bool IsReviewed { get; set; }
    public VideoProcessingStatus ProcessingStatus { get; set; } = VideoProcessingStatus.Pending;

    public Pet Pet { get; set; } = null!;
    public Exercise? Exercise { get; set; }
}
