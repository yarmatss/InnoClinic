namespace Profiles.API.DTOs.MedicalStaff;

public record SetScheduleOverridesDto(List<ScheduleOverrideDto> Overrides);

public record ScheduleOverrideDto(
    DateOnly Date,
    TimeSpan? StartTime,
    TimeSpan? EndTime,
    bool IsDayOff
);
