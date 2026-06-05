using Appointments.API.Features.Shared.GetStaffSchedule;
using Appointments.Domain.Enums;
using Appointments.Infrastructure.Connection;
using Dapper;
using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.GetStaffAvailability;

public class GetStaffAvailabilityHandler(
    ISender sender,
    ISqlConnectionFactory connectionFactory)
    : IRequestHandler<GetStaffAvailabilityQuery, Result<StaffAvailabilityResponse>>
{
    public async Task<Result<StaffAvailabilityResponse>> Handle(
        GetStaffAvailabilityQuery request, 
        CancellationToken cancellationToken)
    {
        var scheduleResult = await sender.Send(
            new GetStaffScheduleQuery(request.MedicalStaffId), 
            cancellationToken);

        if (scheduleResult.IsFailure)
        {
            return scheduleResult.Error;
        }

        var schedule = scheduleResult.Value;

        using var connection = connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT ""StartTime"", ""EndTime""
            FROM ""Appointments""
            WHERE ""MedicalStaffId"" = @MedicalStaffId
              AND ""Status"" != @CancelledStatus
              AND ""StartTime"" < @EndDate
              AND ""EndTime"" > @StartDate";

        var bookedSlots = await connection.QueryAsync<BookedSlotDto>(sql, new
        {
            request.MedicalStaffId,
            CancelledStatus = (int)AppointmentStatus.Cancelled,
            request.StartDate,
            request.EndDate
        });

        var response = new StaffAvailabilityResponse(
            new StaffScheduleDto(
                schedule.WorkingHours.Select(wh => new WorkingHoursDto(
                    wh.DayOfWeek,
                    wh.StartTime,
                    wh.EndTime,
                    wh.IsDayOff)),
                schedule.ScheduleOverrides.Select(so => new ScheduleOverrideDto(
                    so.Date,
                    so.StartTime,
                    so.EndTime,
                    so.IsDayOff))),
            bookedSlots);

        return response;
    }
}
