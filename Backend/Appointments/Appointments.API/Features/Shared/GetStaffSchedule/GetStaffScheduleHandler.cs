using Appointments.API.Extensions;
using Appointments.Domain.Common;
using Appointments.Domain.Constants;
using Appointments.Domain.Interfaces;
using Google.Protobuf;
using Grpc.Core;
using InnoClinic.Contracts.Grpc;
using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.Shared.GetStaffSchedule;

public class GetStaffScheduleHandler(
    ICacheService cacheService,
    StaffScheduleSyncService.StaffScheduleSyncServiceClient profilesClient,
    ILogger<GetStaffScheduleHandler> logger)
    : IRequestHandler<GetStaffScheduleQuery, Result<SyncStaffProfileRequest>>
{
    public async Task<Result<SyncStaffProfileRequest>> Handle(
        GetStaffScheduleQuery request, 
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
        return Result.Success(schedule);
    }
}
