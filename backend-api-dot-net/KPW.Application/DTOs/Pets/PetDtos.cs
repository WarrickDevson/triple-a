namespace KPW.Application.DTOs.Pets;

public record MedicalHistoryDto(
    int MedicalHistoryId,
    string Diagnosis,
    string? InjuryOrCondition,
    DateOnly? SurgeryDate,
    string? ClinicianNotes);

public record PetDto(
    int PetId,
    int OwnerId,
    string OwnerName,
    string PetName,
    string Species,
    string? Breed,
    DateOnly? BirthDate,
    decimal? WeightKg,
    IReadOnlyList<MedicalHistoryDto> MedicalHistories,
    string? ProfilePictureUrl = null);

public record CreateMedicalHistoryDto(
    string Diagnosis,
    string? InjuryOrCondition,
    DateOnly? SurgeryDate,
    string? ClinicianNotes);

public record CreateOwnerDto(
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string TemporaryPassword);

public record CreatePetRequestDto(
    int? OwnerId,
    string PetName,
    string Species,
    string? Breed,
    DateOnly? BirthDate,
    decimal? WeightKg,
    CreateMedicalHistoryDto? InitialMedicalHistory,
    CreateOwnerDto? NewOwner,
    string? ProfilePictureUrl = null);

public record UpdatePetRequestDto(
    string PetName,
    string Species,
    string? Breed,
    DateOnly? BirthDate,
    decimal? WeightKg,
    string? ProfilePictureUrl = null);
