using FluentValidation;
using Profiles.API.DTOs.Patient;
using Profiles.API.Validators.Person;

namespace Profiles.API.Validators.Patient;

public class UpdatePatientDtoValidator : AbstractValidator<UpdatePatientDto>
{
    public UpdatePatientDtoValidator()
    {
        Include(new PersonValidator());

        RuleFor(x => x.BirthDate).SetValidator(new BirthDateValidator());
        RuleFor(x => x.InsuranceNumber).SetValidator(new InsuranceNumberValidator());
        RuleFor(x => x.EmergencyContact).SetValidator(new EmergencyContactValidator());
    }
}
