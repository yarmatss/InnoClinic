using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.SubmitAppointmentResult;

public record SubmitAppointmentResultRequest(
    string Complaints,
    string Conclusion,
    string Recommendations);

public record SubmitAppointmentResultCommand(
    Guid AppointmentId,
    string Complaints,
    string Conclusion,
    string Recommendations) : IRequest<Result<Guid>>;
