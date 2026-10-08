using Appointments.API.Authorization;
using Appointments.API.Constants;
using InnoClinic.AspNetCore.Abstract;
using InnoClinic.AspNetCore.Filters;
using InnoClinic.AspNetCore.Extensions;
using MediatR;
using System.Security.Claims;

namespace Appointments.API.Features.GetStaffAppointments;

public class GetStaffAppointmentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"{ApiRoutes.Appointments}/staff/{{id:guid}}", async (
            Guid id,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var userId = user.GetUserId() ?? string.Empty;

            var query = new GetStaffAppointmentsQuery(userId, id);
            var result = await sender.Send(query, ct);
            return result;
        })
        .WithTags("Appointments")
        .RequireAuthorization(Policies.ReadAppointments)
        .AddEndpointFilter<ResultFilter>();
    }
}
