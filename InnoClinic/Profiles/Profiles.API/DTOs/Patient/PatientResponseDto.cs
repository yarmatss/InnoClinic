using Profiles.Domain.Enums;

namespace Profiles.API.DTOs.Patient;

public record PatientResponseDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly BirthDate,
    Gender Gender,
    string NationalId,
    string? ContactPhone,
    string InsuranceNumber,
    string? EmergencyContact,
    Guid? PrimaryDoctorId);
