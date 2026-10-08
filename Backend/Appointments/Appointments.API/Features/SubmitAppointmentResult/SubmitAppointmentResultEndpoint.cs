using Appointments.API.Authorization;
using Appointments.API.Constants;
using InnoClinic.AspNetCore.Abstract;
using InnoClinic.AspNetCore.Extensions;
using InnoClinic.AspNetCore.Filters;
using MediatR;
using System.Security.Claims;

namespace Appointments.API.Features.SubmitAppointmentResult;

public class SubmitAppointmentResultEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiRoutes.Appointments}/{{id:guid}}/results", async (
            Guid id, 
            ClaimsPrincipal user,
            SubmitAppointmentResultRequest request, 
            ISender sender,
            CancellationToken ct = default) =>
        {
            var userId = user.GetUserId() ?? string.Empty;

            var command = new SubmitAppointmentResultCommand(
                userId,
                id,
                request.Complaints,
                request.Conclusion,
                request.Recommendations);

            var result = await sender.Send(command, ct);
            return result;
        })
        .WithTags("Appointments")
        .RequireAuthorization(Policies.WriteResults)
        .AddEndpointFilter<ResultFilter>();
    }
}
