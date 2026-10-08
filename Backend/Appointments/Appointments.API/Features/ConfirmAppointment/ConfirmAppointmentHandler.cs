using Appointments.Domain.Common;
using Appointments.Domain.Enums;
using Appointments.Infrastructure.Data;
using InnoClinic.Core.Common;
using InnoClinic.Core.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Appointments.API.Features.ConfirmAppointment;

public class ConfirmAppointmentHandler(
    AppointmentsDbContext dbContext,
    IUserResolver userResolver)
    : IRequestHandler<ConfirmAppointmentCommand, Result>
{
    public async Task<Result> Handle(ConfirmAppointmentCommand request, CancellationToken cancellationToken)
    {
        var user = userResolver.Resolve(request.UserId);
        if (user == null)
            return AppointmentErrors.Unauthorized;

        var appointment = await dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment is null)
            return AppointmentErrors.NotFound(request.AppointmentId);

        if (!user.IsAdmin && (!user.IsDoctor || user.StaffId != appointment.MedicalStaffId))
            return AppointmentErrors.Forbidden;

        if (appointment.Status != AppointmentStatus.Planned)
            return AppointmentErrors.CannotConfirm;

        appointment.Status = AppointmentStatus.Confirmed;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
