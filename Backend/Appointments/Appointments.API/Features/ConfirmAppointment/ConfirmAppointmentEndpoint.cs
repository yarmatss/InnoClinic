using Appointments.API.Authorization;
using Appointments.API.Constants;
using InnoClinic.AspNetCore.Abstract;
using InnoClinic.AspNetCore.Filters;
using InnoClinic.AspNetCore.Extensions;
using MediatR;
using System.Security.Claims;

namespace Appointments.API.Features.ConfirmAppointment;

public class ConfirmAppointmentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{ApiRoutes.Appointments}/{{id:guid}}/confirm", async (
            Guid id,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var userId = user.GetUserId() ?? string.Empty;

            var result = await sender.Send(new ConfirmAppointmentCommand(userId, id), ct);
            return result;
        })
        .WithTags("Appointments")
        .RequireAuthorization(Policies.ConfirmAppointments)
        .AddEndpointFilter<ResultFilter>();
    }
}
