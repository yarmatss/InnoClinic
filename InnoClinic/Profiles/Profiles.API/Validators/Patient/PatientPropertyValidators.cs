using FluentValidation;

namespace Profiles.API.Validators.Patient;

internal class InsuranceNumberValidator : AbstractValidator<string>
{
    public InsuranceNumberValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("Insurance number is required.")
            .MaximumLength(50).WithMessage("Insurance number cannot exceed 50 characters.");
    }
}

internal class EmergencyContactValidator : AbstractValidator<string?>
{
    public EmergencyContactValidator()
    {
        RuleFor(x => x)
            .MaximumLength(100).WithMessage("Emergency contact cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x));
    }
}
