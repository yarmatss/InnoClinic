using FluentValidation;

namespace Appointments.API.Features.BookAppointment;

public class BookAppointmentValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty().GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.EndTime).NotEmpty().GreaterThan(x => x.StartTime);
    }
}
