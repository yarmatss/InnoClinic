using Appointments.API.Authorization;
using Appointments.API.Constants;
using InnoClinic.AspNetCore.Abstract;
using InnoClinic.AspNetCore.Filters;
using MediatR;

namespace Appointments.API.Features.GetStaffAppointments;

public class GetStaffAppointmentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"{ApiRoutes.Appointments}/staff/{{id:guid}}", async (
            Guid id,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var query = new GetStaffAppointmentsQuery(id);
            var result = await sender.Send(query, ct);
            return result;
        })
        .WithTags("Appointments")
        .RequireAuthorization(Policies.ReadAppointments)
        .AddEndpointFilter<ResultFilter>();
    }
}
