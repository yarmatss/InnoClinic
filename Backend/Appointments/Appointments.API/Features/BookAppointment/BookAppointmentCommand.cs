using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.BookAppointment;

public record BookAppointmentCommand(
    Guid PatientId,
    Guid MedicalStaffId,
    DateTime StartTime,
    DateTime EndTime,
    string? Comments) : IRequest<Result<Guid>>;
