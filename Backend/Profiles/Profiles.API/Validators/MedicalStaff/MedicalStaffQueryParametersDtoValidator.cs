using FluentValidation;
using Profiles.API.Validators.Common;
using Profiles.Domain.Models;

namespace Profiles.API.Validators.MedicalStaff;

public class MedicalStaffQueryParametersValidator : AbstractValidator<MedicalStaffQueryParameters>
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "FirstName",
        "LastName",
        "StaffType"
    };

    public MedicalStaffQueryParametersValidator()
    {
        Include(new PaginationQueryParametersValidator<MedicalStaffQueryParameters>());

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrEmpty(x) || AllowedSortFields.Contains(x))
            .WithMessage($"SortBy must be one of the following: {string.Join(", ", AllowedSortFields)}");
    }
}
