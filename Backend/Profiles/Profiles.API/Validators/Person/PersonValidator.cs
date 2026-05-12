using FluentValidation;
using Profiles.API.DTOs.Common;

namespace Profiles.API.Validators.Person;

public class PersonValidator : AbstractValidator<IPersonDto>
{
    public PersonValidator()
    {
        RuleFor(x => x.FirstName).SetValidator(new FirstNameValidator());
        RuleFor(x => x.LastName).SetValidator(new LastNameValidator());
        RuleFor(x => x.MiddleName).SetValidator(new MiddleNameValidator());
        RuleFor(x => x.Gender).SetValidator(new GenderValidator());
        RuleFor(x => x.NationalId).SetValidator(new NationalIdValidator());
        RuleFor(x => x.ContactPhone).SetValidator(new ContactPhoneValidator());
    }
}
