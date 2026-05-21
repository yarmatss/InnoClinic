using Grpc.Core;
using InnoClinic.Contracts.Grpc;
using Mapster;
using Profiles.API.Extensions;
using Profiles.DAL.Interfaces;

namespace Profiles.API.GrpcHandlers;

public class StaffMemberQueryHandler(
    IMedicalStaffRepository staffRepository,
    ILogger<StaffMemberQueryHandler> logger)
    : StaffScheduleSyncService.StaffScheduleSyncServiceBase
{
    public override async Task<SyncStaffProfileRequest> GetStaffProfile(
        GetStaffProfileRequest request,
        ServerCallContext context)
    {
        logger.LogProcessingGetStaffProfile(request.MedicalStaffId);

        if (!Guid.TryParse(request.MedicalStaffId, out var staffId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid MedicalStaffId format."));
        }

        var staff = await staffRepository.GetByIdAsync(staffId, context.CancellationToken);

        if (staff is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"MedicalStaff with ID {request.MedicalStaffId} not found."));
        }

        return staff.Adapt<SyncStaffProfileRequest>();
    }
}
