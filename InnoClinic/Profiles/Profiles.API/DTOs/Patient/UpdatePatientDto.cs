namespace Profiles.API.DTOs.Patient;

public record UpdatePatientDto(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly BirthDate,
    string Gender,
    string NationalId,
    string? ContactPhone,
    string InsuranceNumber,
    string? EmergencyContact,
    Guid? PrimaryDoctorId);
