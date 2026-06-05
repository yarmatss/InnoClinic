using Appointments.Domain.Enums;
using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Features.GetPatientAppointments;

public record GetPatientAppointmentsQuery(Guid PatientId) : IRequest<Result<IEnumerable<AppointmentResponse>>>;

public record AppointmentResponse(
    Guid Id,
    Guid PatientId,
    Guid MedicalStaffId,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    string? Comments);
