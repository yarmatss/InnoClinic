using FluentValidation;
using Profiles.API.DTOs.MedicalStaff;

namespace Profiles.API.Validators.MedicalStaff;

public class WorkingHoursDtoValidator : AbstractValidator<WorkingHoursDto>
{
    public WorkingHoursDtoValidator()
    {
        RuleFor(x => x.DayOfWeek).IsInEnum();

        RuleFor(x => x)
            .Must(x => x.IsDayOff || x.StartTime < x.EndTime)
            .WithMessage("StartTime must be before EndTime when it is not a day off.");
    }
}

public class SetWorkingHoursDtoValidator : AbstractValidator<SetWorkingHoursDto>
{
    public SetWorkingHoursDtoValidator()
    {
        RuleForEach(x => x.WorkingHours).SetValidator(new WorkingHoursDtoValidator());

        RuleFor(x => x.WorkingHours)
            .Must(x => x.Select(wh => wh.DayOfWeek).Distinct().Count() == x.Count)
            .WithMessage("Schedules for duplicate days of the week are not allowed.");
    }
}
