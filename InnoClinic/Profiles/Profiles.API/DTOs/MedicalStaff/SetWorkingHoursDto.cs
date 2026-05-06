namespace Profiles.API.DTOs.MedicalStaff;

public record SetWorkingHoursDto(List<WorkingHoursDto> WorkingHours);

public record WorkingHoursDto(
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    bool IsDayOff
);
