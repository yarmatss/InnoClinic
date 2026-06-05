using Appointments.Domain.Common;
using Appointments.Domain.Enums;
using Appointments.Infrastructure.Data;
using InnoClinic.Core.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Appointments.API.Features.CancelAppointment;

public class CancelAppointmentHandler(AppointmentsDbContext dbContext)
    : IRequestHandler<CancelAppointmentCommand, Result>
{
    public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment is null)
        {
            return AppointmentErrors.NotFound(request.AppointmentId);
        }

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
        {
            return AppointmentErrors.CannotCancel;
        }

        appointment.Status = AppointmentStatus.Cancelled;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
