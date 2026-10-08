using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.ConfirmAppointment;

public record ConfirmAppointmentCommand(string UserId, Guid AppointmentId) 
    : IRequest<Result>;
