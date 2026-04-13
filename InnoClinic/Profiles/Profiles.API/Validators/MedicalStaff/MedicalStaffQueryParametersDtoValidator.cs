using FluentValidation;
using Profiles.API.DTOs.MedicalStaff;
using Profiles.API.Validators.Common;

namespace Profiles.API.Validators.MedicalStaff;

public class MedicalStaffQueryParametersDtoValidator : AbstractValidator<MedicalStaffQueryParametersDto>
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "FirstName",
        "LastName",
        "StaffType"
    };

    public MedicalStaffQueryParametersDtoValidator()
    {
        Include(new PaginationQueryParametersValidator<MedicalStaffQueryParametersDto>());

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrEmpty(x) || AllowedSortFields.Contains(x))
            .WithMessage($"SortBy must be one of the following: {string.Join(", ", AllowedSortFields)}");
    }
}
