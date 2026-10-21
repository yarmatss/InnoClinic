using Appointments.API.Constants;
using InnoClinic.AspNetCore.Abstract;
using InnoClinic.AspNetCore.Authorization;
using InnoClinic.AspNetCore.Filters;
using MediatR;

namespace Appointments.API.Features.CancelAppointment;

public class CancelAppointmentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{ApiRoutes.Appointments}/{{id:guid}}/cancel", async (
            Guid id,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(new CancelAppointmentCommand(id), ct);
            return result;
        })
        .WithTags("Appointments")
        .RequirePermission(ClinicPermissions.Appointments.Write)
        .AddEndpointFilter<ResultFilter>();
    }
}
