using FluentValidation;
using Profiles.API.DTOs.Specialization;

namespace Profiles.API.Validators.Specialization;

public class CreateSpecializationDtoValidator : AbstractValidator<CreateSpecializationDto>
{
    public CreateSpecializationDtoValidator()
    {
        RuleFor(x => x.Name).SetValidator(new SpecializationNameValidator());
        RuleFor(x => x.Code).SetValidator(new SpecializationCodeValidator());
    }
}
