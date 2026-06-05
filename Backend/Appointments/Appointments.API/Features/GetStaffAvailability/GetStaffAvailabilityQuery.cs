using MediatR;
using InnoClinic.Core.Common;

namespace Appointments.API.Features.GetStaffAvailability;

public record GetStaffAvailabilityQuery(
    Guid MedicalStaffId,
    DateTime StartDate,
    DateTime EndDate) : IRequest<Result<StaffAvailabilityResponse>>;

public record StaffAvailabilityResponse(
    StaffScheduleDto Schedule,
    IEnumerable<BookedSlotDto> BookedSlots);

public record StaffScheduleDto(
    IEnumerable<WorkingHoursDto> WorkingHours,
    IEnumerable<ScheduleOverrideDto> ScheduleOverrides);

public record WorkingHoursDto(
    int DayOfWeek,
    string StartTime,
    string EndTime,
    bool IsDayOff);

public record ScheduleOverrideDto(
    string Date,
    string StartTime,
    string EndTime,
    bool IsDayOff);

public record BookedSlotDto(DateTime StartTime, DateTime EndTime);
