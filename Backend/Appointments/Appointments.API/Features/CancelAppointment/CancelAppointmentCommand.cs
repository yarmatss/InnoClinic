using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.CancelAppointment;

public record CancelAppointmentCommand(string UserId, Guid AppointmentId) 
    : IRequest<Result>;
