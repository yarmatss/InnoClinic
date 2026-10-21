using Appointments.API.Constants;
using InnoClinic.AspNetCore.Abstract;
using InnoClinic.AspNetCore.Authorization;
using InnoClinic.AspNetCore.Filters;
using MediatR;

namespace Appointments.API.Features.BookAppointment;

public class BookAppointmentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiRoutes.Appointments}/book", async (
            BookAppointmentCommand command,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(command, ct);
            return result;
        })
        .WithTags("Appointments")
        .RequirePermission(ClinicPermissions.Appointments.Write)
        .AddEndpointFilter<ResultFilter>();
    }
}
