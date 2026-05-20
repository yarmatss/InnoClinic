using Appointments.API.Extensions;
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
using Grpc.Core;
using Npgsql;
using Appointments.Domain.Constants;

namespace Appointments.API.Features.BookAppointment;

public class BookAppointmentHandler(
    AppointmentsDbContext dbContext,
    IConnectionMultiplexer redis,
    StaffScheduleSyncService.StaffScheduleSyncServiceClient profilesClient,
    IConfiguration configuration,
    ILogger<BookAppointmentHandler> logger)
    : IRequestHandler<BookAppointmentCommand, Result<Guid>>
{
    private const string ClinicTimeZoneConfigKey = "ClinicOptions:TimeZone";
    private const string PostgresExclusionViolationCode = "23P01";

    public async Task<Result<Guid>> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        var redisDb = redis.GetDatabase();
        var redisKey = CacheConstants.MedicalStaffScheduleKey(request.MedicalStaffId);
        var scheduleJson = await redisDb.StringGetAsync(redisKey);

        if (scheduleJson.IsNullOrEmpty)
        {
            logger.LogCacheMissFallback(request.MedicalStaffId);
            
            try
            {
                var fallbackSchedule = await profilesClient.GetStaffProfileAsync(
                    new GetStaffProfileRequest { MedicalStaffId = request.MedicalStaffId.ToString() }, 
                    cancellationToken: cancellationToken);

                scheduleJson = JsonFormatter.Default.Format(fallbackSchedule);
                await redisDb.StringSetAsync(redisKey, scheduleJson);
                
                logger.LogCacheRepopulated(request.MedicalStaffId);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                return AppointmentErrors.MedicalStaffNotFound(request.MedicalStaffId);
            }
            catch (Exception ex)
            {
                logger.LogFallbackError(ex, request.MedicalStaffId);
                throw;
            }
        }

        var schedule = JsonParser.Default.Parse<SyncStaffProfileRequest>(scheduleJson);

        var clinicTimeZoneId = configuration[ClinicTimeZoneConfigKey] ?? "UTC";
        var clinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clinicTimeZoneId);

        var validationResult = ValidateAgainstSchedule(request, schedule, clinicTimeZone);
        if (validationResult.IsFailure)
        {
            return validationResult.Error;
        }

        try
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                MedicalStaffId = request.MedicalStaffId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = AppointmentStatus.Planned,
                Comments = request.Comments
            };

            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync(cancellationToken);

            return appointment.Id;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && 
            pgEx.SqlState == PostgresExclusionViolationCode)
        {
            return AppointmentErrors.ScheduleConflict;
        }
        catch (Exception ex)
        {
            logger.LogBookingError(ex, request.MedicalStaffId);
            throw;
        }
    }

    private static Result ValidateAgainstSchedule(
        BookAppointmentCommand request, 
        SyncStaffProfileRequest schedule,
        TimeZoneInfo clinicTimeZone)
    {
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(request.StartTime, clinicTimeZone);
        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(request.EndTime, clinicTimeZone);

        var dateStr = localStart.ToString("yyyy-MM-dd");
        var overrideMatch = schedule.ScheduleOverrides.FirstOrDefault(o => o.Date == dateStr);

        if (overrideMatch != null)
        {
            if (overrideMatch.IsDayOff)
            {
                return AppointmentErrors.MedicalStaffOnDayOff;
            }

            if (!IsWithinRange(localStart, localEnd, overrideMatch.StartTime, overrideMatch.EndTime))
            {
                return AppointmentErrors.OutsideWorkingHours;
            }

            return Result.Success();
        }

        var dayOfWeek = (int)localStart.DayOfWeek;
        var workingHours = schedule.WorkingHours.FirstOrDefault(w => w.DayOfWeek == dayOfWeek);

        if (workingHours == null || workingHours.IsDayOff)
        {
            return AppointmentErrors.MedicalStaffOnDayOff;
        }

        if (!IsWithinRange(localStart, localEnd, workingHours.StartTime, workingHours.EndTime))
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
