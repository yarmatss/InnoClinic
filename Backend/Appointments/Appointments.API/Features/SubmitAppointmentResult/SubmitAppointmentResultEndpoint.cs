using Appointments.API.Authorization;
using Appointments.API.Constants;
using InnoClinic.AspNetCore.Abstract;
using InnoClinic.AspNetCore.Filters;
using MediatR;

namespace Appointments.API.Features.SubmitAppointmentResult;

public class SubmitAppointmentResultEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiRoutes.Appointments}/{{id:guid}}/results", async (
            Guid id, 
            SubmitAppointmentResultRequest request, 
            ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new SubmitAppointmentResultCommand(
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
