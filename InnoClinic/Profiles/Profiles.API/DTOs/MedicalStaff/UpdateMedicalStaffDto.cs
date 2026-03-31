using Profiles.Domain.Enums;

namespace Profiles.API.DTOs.MedicalStaff;

public record UpdateMedicalStaffDto(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly BirthDate,
    string Gender,
    string NationalId,
    string? ContactPhone,
    StaffType StaffType,
    string LicenseNumber,
    bool IsActive,
    Guid? SupervisorId);
