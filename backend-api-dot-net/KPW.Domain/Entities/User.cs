using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class User : AuditableEntity
{
    public int UserId { get; set; }
    public int? ClinicId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string UserRole { get; set; } = Enums.UserRole.Owner;
    public string SubscriptionTier { get; set; } = Enums.SubscriptionTier.Free;
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationTokenHash { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    public bool IsApproved { get; set; } = true;

    public Clinic? Clinic { get; set; }
    public ICollection<Pet> Pets { get; set; } = [];
    public ICollection<RehabProgram> RehabProgramsAsPhysio { get; set; } = [];
    public ICollection<Appointment> AppointmentsAsPhysio { get; set; } = [];
    public ICollection<Appointment> AppointmentsAsOwner { get; set; } = [];
}
