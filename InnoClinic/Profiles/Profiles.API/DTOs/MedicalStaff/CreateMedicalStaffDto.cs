using Profiles.Domain.Enums;

namespace Profiles.API.DTOs.MedicalStaff;

public record CreateMedicalStaffDto(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly BirthDate,
    string Gender,
    string NationalId,
    string? ContactPhone,
    StaffType StaffType,
    string LicenseNumber,
    DateOnly HireDate,
    Guid? SupervisorId);
