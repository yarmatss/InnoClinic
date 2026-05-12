using FluentValidation;
using Profiles.API.DTOs.MedicalStaff;
using Profiles.API.Validators.Specialization;

namespace Profiles.API.Validators.MedicalStaff;

public class StaffSpecializationDtoValidator : AbstractValidator<StaffSpecializationDto>
{
    public StaffSpecializationDtoValidator()
    {
        RuleFor(x => x.SpecializationId).SetValidator(new SpecializationIdValidator());
        RuleFor(x => x.CertificationDate).SetValidator(new CertificationDateValidator());
    }
}
