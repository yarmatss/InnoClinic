using Appointments.API.Authorization;
using Appointments.Domain.Common;
using Appointments.Domain.Entities;
using Appointments.Domain.Enums;
using Appointments.Infrastructure.Data;
using InnoClinic.Core.Authorization;
using InnoClinic.Core.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Appointments.API.Features.SubmitAppointmentResult;

public class SubmitAppointmentResultHandler(
    AppointmentsDbContext dbContext,
    IAccessResolver accessResolver)
    : IRequestHandler<SubmitAppointmentResultCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SubmitAppointmentResultCommand request, CancellationToken cancellationToken)
    {
        var appointment = await dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment is null || !accessResolver.Check(appointment, AppointmentOperations.SubmitResult).IsAllowed)
            return AppointmentErrors.NotFound(request.AppointmentId);

        if (appointment.Status == AppointmentStatus.Cancelled)
            return AppointmentErrors.CannotSubmitResult;

        var existingResult = await dbContext.AppointmentResults
            .AnyAsync(r => r.AppointmentId == request.AppointmentId, cancellationToken);

        if (existingResult)
            return AppointmentErrors.ResultAlreadyExists;

        var result = new AppointmentResult
        {
            Id = Guid.NewGuid(),
            AppointmentId = request.AppointmentId,
            Complaints = request.Complaints,
            Conclusion = request.Conclusion,
            Recommendations = request.Recommendations
        };

        appointment.Status = AppointmentStatus.Completed;

        dbContext.AppointmentResults.Add(result);
        await dbContext.SaveChangesAsync(cancellationToken);

        return result.Id;
    }
}
