namespace Profiles.API.DTOs.Specialization;

public record SpecializationResponseDto(
    Guid Id,
    string Name,
    string? Code);
