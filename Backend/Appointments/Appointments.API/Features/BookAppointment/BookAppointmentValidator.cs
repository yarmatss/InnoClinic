using FluentValidation;

namespace Appointments.API.Features.BookAppointment;

public class BookAppointmentValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.MedicalStaffId).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty().GreaterThan(_ => timeProvider.GetUtcNow().UtcDateTime);
        RuleFor(x => x.EndTime).NotEmpty().GreaterThan(x => x.StartTime);
    }
}
