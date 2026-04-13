using FluentValidation;
using Profiles.API.DTOs.MedicalStaff;

namespace Profiles.API.Validators.MedicalStaff;

public class AssignSpecializationsDtoValidator : AbstractValidator<AssignSpecializationsDto>
{
    public AssignSpecializationsDtoValidator()
    {
        RuleFor(x => x.Specializations)
            .NotNull()
            .Must(specializations => specializations.Count(s => s.IsPrimary) <= 1)
            .WithMessage("Only one specialization can be primary.");

        RuleForEach(x => x.Specializations)
            .SetValidator(new StaffSpecializationDtoValidator());
    }
}
