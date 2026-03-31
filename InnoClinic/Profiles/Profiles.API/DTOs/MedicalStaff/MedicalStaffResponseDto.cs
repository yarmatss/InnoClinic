using Profiles.Domain.Enums;

namespace Profiles.API.DTOs.MedicalStaff;

public record MedicalStaffResponseDto(
    Guid Id,
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
    bool IsActive,
    Guid? SupervisorId,
    IReadOnlyList<StaffSpecializationDto>? StaffSpecializations);
