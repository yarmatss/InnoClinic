using Appointments.Domain.Common;
using Appointments.Domain.Enums;
using Appointments.Infrastructure.Data;
using InnoClinic.Core.Common;
using InnoClinic.Core.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Appointments.API.Features.CancelAppointment;

public class CancelAppointmentHandler(
    AppointmentsDbContext dbContext,
    IUserResolver userResolver)
    : IRequestHandler<CancelAppointmentCommand, Result>
{
    public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var user = userResolver.Resolve(request.UserId);
        if (user is null)
            return AppointmentErrors.Unauthorized;

        var appointment = await dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment is null)
            return AppointmentErrors.NotFound(request.AppointmentId);

        if (user.Role == UserRole.Patient && user.PatientId != appointment.PatientId)
            return AppointmentErrors.Forbidden;

        if (user.Role == UserRole.Doctor && user.StaffId != appointment.MedicalStaffId)
            return AppointmentErrors.Forbidden;

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            return AppointmentErrors.CannotCancel;

        appointment.Status = AppointmentStatus.Cancelled;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
