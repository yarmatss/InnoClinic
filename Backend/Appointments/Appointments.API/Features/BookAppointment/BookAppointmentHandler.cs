using Appointments.API.Extensions;
using Appointments.API.Options;
using Appointments.Domain.Common;
using Appointments.Domain.Constants;
using Appointments.Domain.Entities;
using Appointments.Domain.Enums;
using Appointments.Domain.Exceptions;
using Appointments.Domain.Interfaces;
using Appointments.Infrastructure.Data;
using Google.Protobuf;
using Grpc.Core;
using InnoClinic.Contracts.Grpc;
using InnoClinic.Core.Common;
using MediatR;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Appointments.API.Features.BookAppointment;

public class BookAppointmentHandler(
    AppointmentsDbContext dbContext,
    ICacheService cacheService,
    StaffScheduleSyncService.StaffScheduleSyncServiceClient profilesClient,
    PatientService.PatientServiceClient patientClient,
    IOptions<ClinicOptions> clinicOptions,
    ILogger<BookAppointmentHandler> logger)
    : IRequestHandler<BookAppointmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        var patientResponse = await patientClient.GetPatientAsync(
            new GetPatientRequest { PatientId = request.PatientId.ToString() },
            cancellationToken: cancellationToken);

        if (!patientResponse.Exists)
        {
            return AppointmentErrors.PatientNotFound(request.PatientId);
        }

        var redisKey = CacheConstants.MedicalStaffScheduleKey(request.MedicalStaffId);

        var scheduleJson = await cacheService.GetOrSetStringAsync(
            redisKey,
            async (ct) =>
            {
                logger.LogCacheMissFallback(request.MedicalStaffId);

                try
                {
                    var fallbackSchedule = await profilesClient.GetStaffProfileAsync(
                        new GetStaffProfileRequest { MedicalStaffId = request.MedicalStaffId.ToString() },
                        cancellationToken: ct);

                    var json = JsonFormatter.Default.Format(fallbackSchedule);
                    logger.LogCacheRepopulated(request.MedicalStaffId);
                    return json;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    logger.LogFallbackError(ex, request.MedicalStaffId);
                    throw;
                }
            },
            cancellationToken: cancellationToken);

        if (string.IsNullOrEmpty(scheduleJson))
        {
            return AppointmentErrors.MedicalStaffNotFound(request.MedicalStaffId);
        }

        var schedule = JsonParser.Default.Parse<SyncStaffProfileRequest>(scheduleJson);

        var clinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clinicOptions.Value.TimeZone);

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
        catch (ScheduleConflictException)
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
