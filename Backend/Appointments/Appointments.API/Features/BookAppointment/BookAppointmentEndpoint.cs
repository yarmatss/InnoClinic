using InnoClinic.AspNetCore.Abstract;
using InnoClinic.AspNetCore.Filters;
using MediatR;

namespace Appointments.API.Features.BookAppointment;

public class BookAppointmentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/appointments/book", async (BookAppointmentCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result;
        })
        .WithTags("Appointments")
        .AddEndpointFilter<ResultFilter>();
    }
}
