using Google.Protobuf;
using Grpc.Core;
using InnoClinic.Shared.Protos;
using StackExchange.Redis;

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
        logger.LogInformation("Processing sync for Medical Staff: {StaffId}", request.MedicalStaffId);

        try
        {
            var db = redis.GetDatabase();
            var redisKey = $"medicalstaff:schedule:{request.MedicalStaffId}";

            if (!request.IsActive)
            {
                await db.KeyDeleteAsync(redisKey);
                logger.LogInformation("Staff {StaffId} deactivated. Entry removed from Redis.", request.MedicalStaffId);
            }
            else
            {
                var jsonPayload = JsonFormatter.Default.Format(request);

                await db.StringSetAsync(redisKey, jsonPayload);
                logger.LogInformation("Successfully updated Redis for Staff: {StaffId}", request.MedicalStaffId);
            }

            return new SyncStaffProfileResponse { Success = true };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync Staff: {StaffId} to Redis.", request.MedicalStaffId);
            throw new RpcException(new Status(StatusCode.Internal, "Redis synchronization failed."));
        }
    }
}
