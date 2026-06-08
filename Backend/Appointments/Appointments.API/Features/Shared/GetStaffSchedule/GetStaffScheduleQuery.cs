using InnoClinic.Contracts.Grpc;
using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.Shared.GetStaffSchedule;

public record GetStaffScheduleQuery(Guid MedicalStaffId) : IRequest<Result<SyncStaffProfileRequest>>;
