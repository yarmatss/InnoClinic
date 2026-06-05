using FluentValidation;

namespace Appointments.API.Features.SubmitAppointmentResult;

public class SubmitAppointmentResultValidator : AbstractValidator<SubmitAppointmentResultCommand>
{
    public SubmitAppointmentResultValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Complaints).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Conclusion).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Recommendations).NotEmpty().MaximumLength(1000);
    }
}
