using Appointments.Domain.Common;
using Appointments.Domain.Entities;
using Appointments.Domain.Enums;
using Appointments.Infrastructure.Data;
using Google.Protobuf;
using InnoClinic.Core.Common;
using InnoClinic.Contracts.Grpc;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Globalization;

namespace Appointments.API.Features.BookAppointment;

public class BookAppointmentHandler(
    AppointmentsDbContext dbContext,
    IConnectionMultiplexer redis,
    ILogger<BookAppointmentHandler> logger)
    : IRequestHandler<BookAppointmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        var redisDb = redis.GetDatabase();
        var redisKey = $"medicalstaff:schedule:{request.DoctorId}";
        var scheduleJson = await redisDb.StringGetAsync(redisKey);

        if (scheduleJson.IsNullOrEmpty)
        {
            return AppointmentErrors.DoctorNotFound(request.DoctorId);
        }

        var schedule = JsonParser.Default.Parse<SyncStaffProfileRequest>(scheduleJson);

        var validationResult = ValidateAgainstSchedule(request, schedule);
        if (validationResult.IsFailure)
        {
            return validationResult.Error;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var isOverlapping = await dbContext.Appointments
                .AnyAsync(a => a.DoctorId == request.DoctorId &&
                               a.Status != AppointmentStatus.Cancelled &&
                               a.StartTime < request.EndTime &&
                               request.StartTime < a.EndTime,
                    cancellationToken);

            if (isOverlapping)
            {
                return AppointmentErrors.ScheduleConflict;
            }

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = AppointmentStatus.Planned,
                Comments = request.Comments
            };

            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return appointment.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error booking appointment for doctor {DoctorId}", request.DoctorId);
            throw;
        }
    }

    private static Result ValidateAgainstSchedule(BookAppointmentCommand request, SyncStaffProfileRequest schedule)
    {
        var dateStr = request.StartTime.ToString("yyyy-MM-dd");
        var overrideMatch = schedule.ScheduleOverrides.FirstOrDefault(o => o.Date == dateStr);

        if (overrideMatch != null)
        {
            if (overrideMatch.IsDayOff)
            {
                return AppointmentErrors.DoctorOnDayOff;
            }

            if (!IsWithinRange(request.StartTime, request.EndTime, overrideMatch.StartTime, overrideMatch.EndTime))
            {
                return AppointmentErrors.OutsideWorkingHours;
            }

            return Result.Success();
        }

        var dayOfWeek = (int)request.StartTime.DayOfWeek;
        var workingHours = schedule.WorkingHours.FirstOrDefault(w => w.DayOfWeek == dayOfWeek);

        if (workingHours == null || workingHours.IsDayOff)
        {
            return AppointmentErrors.DoctorOnDayOff;
        }

        if (!IsWithinRange(request.StartTime, request.EndTime, workingHours.StartTime, workingHours.EndTime))
        {
            return AppointmentErrors.OutsideWorkingHours;
        }

        return Result.Success();
    }

    private static bool IsWithinRange(DateTime start, DateTime end, string rangeStartStr, string rangeEndStr)
    {
        if (!TimeSpan.TryParse(rangeStartStr, CultureInfo.InvariantCulture, out var rangeStart) ||
            !TimeSpan.TryParse(rangeEndStr, CultureInfo.InvariantCulture, out var rangeEnd))
        {
            return false;
        }

        var requestStart = start.TimeOfDay;
        var requestEnd = end.TimeOfDay;

        return requestStart >= rangeStart && requestEnd <= rangeEnd;
    }
}
