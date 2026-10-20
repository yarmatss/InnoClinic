using Appointments.API.Constants;
using InnoClinic.AspNetCore.Abstract;
using InnoClinic.AspNetCore.Authorization;
using InnoClinic.AspNetCore.Filters;
using MediatR;

namespace Appointments.API.Features.GetPatientAppointments;

public class GetPatientAppointmentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"{ApiRoutes.Appointments}/patient/{{id:guid}}", async (
            Guid id,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var query = new GetPatientAppointmentsQuery(id);
            var result = await sender.Send(query, ct);
            return result;
        })
        .WithTags("Appointments")
        .RequirePermission(ClinicPermissions.Appointments.Read)
        .AddEndpointFilter<ResultFilter>();
    }
}
