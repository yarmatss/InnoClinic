using FluentValidation;
using Profiles.API.DTOs.MedicalStaff;
using Profiles.API.Validators.Person;

namespace Profiles.API.Validators.MedicalStaff;

public class CreateMedicalStaffDtoValidator : AbstractValidator<CreateMedicalStaffDto>
{
    public CreateMedicalStaffDtoValidator()
    {
        Include(new PersonValidator());

        RuleFor(x => x.BirthDate).SetValidator(new MedicalStaffBirthDateValidator());
        RuleFor(x => x.StaffType).SetValidator(new StaffTypeValidator());
        RuleFor(x => x.LicenseNumber).SetValidator(new LicenseNumberValidator());
        RuleFor(x => x.HireDate).SetValidator(new HireDateValidator());
    }
}
