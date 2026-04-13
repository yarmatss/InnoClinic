using FluentValidation;

namespace Profiles.API.Validators.Specialization;

internal class SpecializationNameValidator : AbstractValidator<string>
{
    public SpecializationNameValidator()
    {
        RuleFor(name => name)
            .NotEmpty().WithMessage("Specialization name is required.")
            .MaximumLength(100).WithMessage("Specialization name cannot exceed 100 characters.");
    }
}

internal class SpecializationCodeValidator : AbstractValidator<string?>
{
    public SpecializationCodeValidator()
    {
        RuleFor(code => code)
            .MaximumLength(50).WithMessage("Specialization code cannot exceed 50 characters.");
    }
}

internal class SpecializationIdValidator : AbstractValidator<Guid>
{
    public SpecializationIdValidator()
    {
        RuleFor(id => id).NotEmpty().WithMessage("Specialization ID is required.");
    }
}
