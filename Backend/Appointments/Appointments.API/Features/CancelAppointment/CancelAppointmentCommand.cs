using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.CancelAppointment;

public record CancelAppointmentCommand(Guid AppointmentId) : IRequest<Result>;
