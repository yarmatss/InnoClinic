using Appointments.API.Constants;
using InnoClinic.AspNetCore.Abstract;
using InnoClinic.AspNetCore.Authorization;
using InnoClinic.AspNetCore.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Appointments.API.Features.GetStaffAvailability;

public class GetStaffAvailabilityEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"{ApiRoutes.Appointments}/availability/{{id:guid}}", async (
            Guid id,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var query = new GetStaffAvailabilityQuery(id, startDate, endDate);
            var result = await sender.Send(query, ct);
            return result;
        })
        .WithTags("Appointments")
        .RequirePermission(ClinicPermissions.Appointments.Read)
        .AddEndpointFilter<ResultFilter>();
    }
}
