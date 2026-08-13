using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class Pet : AuditableEntity
{
    public int PetId { get; set; }
    public int OwnerId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public DateOnly? BirthDate { get; set; }
    public decimal? WeightKg { get; set; }

    public User Owner { get; set; } = null!;
    public ICollection<MedicalHistory> MedicalHistories { get; set; } = [];
    public ICollection<RehabProgram> RehabPrograms { get; set; } = [];
    public ICollection<DailyTrackingLog> DailyTrackingLogs { get; set; } = [];
    public ICollection<VideoSubmission> VideoSubmissions { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
    public MessageThread? MessageThread { get; set; }
    public ICollection<ExerciseSessionLog> ExerciseSessionLogs { get; set; } = [];
    public ICollection<SoapNote> SoapNotes { get; set; } = [];
    public ICollection<SharedReport> SharedReports { get; set; } = [];
    public ICollection<OwnerSubjectiveNote> OwnerSubjectiveNotes { get; set; } = [];
}
