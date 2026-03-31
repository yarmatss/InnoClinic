namespace Profiles.API.DTOs.MedicalStaff;

public record StaffSpecializationDto(
    Guid SpecializationId,
    bool IsPrimary,
    DateOnly CertificationDate);
