using Appointments.API.Extensions;
using StackExchange.Redis;
using InnoClinic.Contracts.Grpc;
using Grpc.Core;
using Google.Protobuf;

namespace Appointments.API.GrpcHandlers;

public class StaffScheduleSyncHandler(
    IConnectionMultiplexer redis,
    ILogger<StaffScheduleSyncHandler> logger)
    : StaffScheduleSyncService.StaffScheduleSyncServiceBase
{
    public override async Task<SyncStaffProfileResponse> SyncStaffProfile(
        SyncStaffProfileRequest request,
        ServerCallContext context)
    {
        logger.LogProcessingSync(request.MedicalStaffId);

        try
        {
            var db = redis.GetDatabase();
            var redisKey = $"medicalstaff:schedule:{request.MedicalStaffId}";

            if (!request.IsActive)
            {
                await db.KeyDeleteAsync(redisKey);
                logger.LogStaffDeactivated(request.MedicalStaffId);
            }
            else
            {
                var jsonPayload = JsonFormatter.Default.Format(request);

                await db.StringSetAsync(redisKey, jsonPayload);
                logger.LogSyncSuccess(request.MedicalStaffId);
            }

            return new SyncStaffProfileResponse { Success = true };
        }
        catch (Exception ex)
        {
            logger.LogSyncError(ex, request.MedicalStaffId);
            throw new RpcException(new Status(StatusCode.Internal, "Redis synchronization failed."));
        }
    }
}
