using FluentValidation;

namespace Appointments.API.Features.GetStaffAvailability;

public class GetStaffAvailabilityValidator : AbstractValidator<GetStaffAvailabilityQuery>
{
    public GetStaffAvailabilityValidator()
    {
        RuleFor(x => x.MedicalStaffId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate);
    }
}
