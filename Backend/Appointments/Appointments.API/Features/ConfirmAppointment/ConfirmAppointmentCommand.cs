using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.ConfirmAppointment;

public record ConfirmAppointmentCommand(Guid AppointmentId) : IRequest<Result>;
