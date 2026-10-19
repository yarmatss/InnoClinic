using Appointments.Domain.Common;
using Appointments.Domain.Enums;
using Appointments.Infrastructure.Data;
using InnoClinic.Core.Common;
using Appointments.API.Authorization;
using InnoClinic.Core.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Appointments.API.Features.ConfirmAppointment;

public class ConfirmAppointmentHandler(
    AppointmentsDbContext dbContext,
    IAccessResolver accessResolver)
    : IRequestHandler<ConfirmAppointmentCommand, Result>
{
    public async Task<Result> Handle(ConfirmAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment is null || !accessResolver.Check(appointment, AppointmentOperations.Confirm).IsAllowed)
            return AppointmentErrors.NotFound(request.AppointmentId);

        if (appointment.Status != AppointmentStatus.Planned)
            return AppointmentErrors.CannotConfirm;

        appointment.Status = AppointmentStatus.Confirmed;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
