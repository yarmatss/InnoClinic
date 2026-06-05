using Grpc.Core;
using InnoClinic.Contracts.Grpc;
using Profiles.DAL.Interfaces;

namespace Profiles.API.GrpcHandlers;

public class PatientQueryHandler(
    IPatientRepository patientRepository)
    : PatientService.PatientServiceBase
{
    public override async Task<GetPatientResponse> GetPatient(
        GetPatientRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.PatientId, out var patientId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid PatientId format."));
        }

        var patient = await patientRepository.GetByIdAsync(patientId, context.CancellationToken);

        return new GetPatientResponse
        {
            PatientId = request.PatientId,
            Exists = patient is not null
        };
    }
}
