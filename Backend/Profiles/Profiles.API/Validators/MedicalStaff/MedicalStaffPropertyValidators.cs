using FluentValidation;
using Profiles.Domain.Enums;

namespace Profiles.API.Validators.MedicalStaff;

internal class StaffTypeValidator : AbstractValidator<StaffType>
{
    public StaffTypeValidator()
    {
        RuleFor(x => x)
            .IsInEnum().WithMessage("Invalid Staff Type provided.");
    }
}

internal class LicenseNumberValidator : AbstractValidator<string>
{
    public LicenseNumberValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("License number is required.")
            .MaximumLength(50).WithMessage("License number cannot exceed 50 characters.");
    }
}

internal class HireDateValidator : AbstractValidator<DateOnly>
{
    public HireDateValidator()
    {
        RuleFor(x => x)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Hire date cannot be in the future.");
    }
}

internal class MedicalStaffBirthDateValidator : AbstractValidator<DateOnly>
{
    public MedicalStaffBirthDateValidator()
    {
        RuleFor(date => date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18))
            .WithMessage("Staff member must be at least 18 years old.");
    }
}

internal class CertificationDateValidator : AbstractValidator<DateOnly>
{
    public CertificationDateValidator()
    {
        RuleFor(x => x)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Certification date cannot be in the future.");
    }
}
