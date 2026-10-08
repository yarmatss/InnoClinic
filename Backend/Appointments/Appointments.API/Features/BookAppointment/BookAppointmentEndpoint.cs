using Appointments.API.Authorization;
using Appointments.API.Constants;
using InnoClinic.AspNetCore.Abstract;
using InnoClinic.AspNetCore.Extensions;
using InnoClinic.AspNetCore.Filters;
using MediatR;
using System.Security.Claims;

namespace Appointments.API.Features.BookAppointment;

public class BookAppointmentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiRoutes.Appointments}/book", async (
            ClaimsPrincipal user,
            BookAppointmentCommand command,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var userId = user.GetUserId() ?? string.Empty;

            var commandWithUser = command with { UserId = userId };
            var result = await sender.Send(commandWithUser, ct);
            return result;
        })
        .WithTags("Appointments")
        .RequireAuthorization(Policies.WriteAppointments)
        .AddEndpointFilter<ResultFilter>();
    }
}
