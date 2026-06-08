using Appointments.API.Authorization;
using Appointments.API.Constants;
using InnoClinic.AspNetCore.Abstract;
using InnoClinic.AspNetCore.Filters;
using MediatR;

namespace Appointments.API.Features.ConfirmAppointment;

public class ConfirmAppointmentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{ApiRoutes.Appointments}/{{id:guid}}/confirm", async (
            Guid id,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(new ConfirmAppointmentCommand(id), ct);
            return result;
        })
        .WithTags("Appointments")
        .RequireAuthorization(Policies.ConfirmAppointments)
        .AddEndpointFilter<ResultFilter>();
    }
}
