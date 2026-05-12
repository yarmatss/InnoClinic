using FluentValidation;
using Profiles.Domain.Enums;

namespace Profiles.API.Validators.Person;

internal class FirstNameValidator : AbstractValidator<string>
{
    public FirstNameValidator()
    {
        RuleFor(name => name)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");
    }
}

internal class LastNameValidator : AbstractValidator<string>
{
    public LastNameValidator()
    {
        RuleFor(name => name)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");
    }
}

internal class MiddleNameValidator : AbstractValidator<string?>
{
    public MiddleNameValidator()
    {
        RuleFor(name => name)
            .MaximumLength(50).WithMessage("Middle name cannot exceed 50 characters.")
            .When(name => !string.IsNullOrWhiteSpace(name));
    }
}

internal class BirthDateValidator : AbstractValidator<DateOnly>
{
    public BirthDateValidator()
    {
        RuleFor(date => date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Birth date cannot be in the future.");
    }
}

internal class GenderValidator : AbstractValidator<Gender>
{
    public GenderValidator()
    {
        RuleFor(gender => gender)
            .IsInEnum().WithMessage("Invalid Gender provided.");
    }
}

internal class NationalIdValidator : AbstractValidator<string>
{
    public NationalIdValidator()
    {
        RuleFor(id => id)
            .NotEmpty().WithMessage("National ID is required.")
            .MaximumLength(50).WithMessage("National ID cannot exceed 50 characters.");
    }
}

internal class ContactPhoneValidator : AbstractValidator<string?>
{
    public ContactPhoneValidator()
    {
        RuleFor(phone => phone)
            .MaximumLength(20).WithMessage("Contact phone cannot exceed 20 characters.")
            .When(phone => !string.IsNullOrWhiteSpace(phone));
    }
}
