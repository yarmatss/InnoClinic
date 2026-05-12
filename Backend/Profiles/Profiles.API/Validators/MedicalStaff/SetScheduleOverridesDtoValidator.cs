using FluentValidation;
using Profiles.API.DTOs.MedicalStaff;

namespace Profiles.API.Validators.MedicalStaff;

public class ScheduleOverrideDtoValidator : AbstractValidator<ScheduleOverrideDto>
{
    public ScheduleOverrideDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => x.IsDayOff || (x.StartTime.HasValue && x.EndTime.HasValue && x.StartTime < x.EndTime))
            .WithMessage("StartTime and EndTime are required and StartTime must be before EndTime when it is not a day off.");
    }
}

public class SetScheduleOverridesDtoValidator : AbstractValidator<SetScheduleOverridesDto>
{
    public SetScheduleOverridesDtoValidator()
    {
        RuleForEach(x => x.Overrides).SetValidator(new ScheduleOverrideDtoValidator());

        RuleFor(x => x.Overrides)
            .Must(x => x.Select(so => so.Date).Distinct().Count() == x.Count)
            .WithMessage("Schedules for duplicate dates are not allowed in a single payload.");
    }
}
