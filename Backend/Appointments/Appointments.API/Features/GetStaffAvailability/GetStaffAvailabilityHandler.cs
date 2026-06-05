using Appointments.API.Extensions;
using Appointments.Domain.Common;
using Appointments.Domain.Constants;
using Appointments.Domain.Enums;
using Appointments.Domain.Interfaces;
using Appointments.Infrastructure.Connection;
using Dapper;
using Google.Protobuf;
using Grpc.Core;
using InnoClinic.Contracts.Grpc;
using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.GetStaffAvailability;

public class GetStaffAvailabilityHandler(
    ICacheService cacheService,
    StaffScheduleSyncService.StaffScheduleSyncServiceClient profilesClient,
    ISqlConnectionFactory connectionFactory,
    ILogger<GetStaffAvailabilityHandler> logger)
    : IRequestHandler<GetStaffAvailabilityQuery, Result<StaffAvailabilityResponse>>
{
    public async Task<Result<StaffAvailabilityResponse>> Handle(
        GetStaffAvailabilityQuery request, 
        CancellationToken cancellationToken)
    {
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
