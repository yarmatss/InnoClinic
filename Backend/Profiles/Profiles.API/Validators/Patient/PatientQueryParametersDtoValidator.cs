using FluentValidation;
using Profiles.Domain.Models;
using Profiles.API.Validators.Common;

namespace Profiles.API.Validators.Patient;

public class PatientQueryParametersValidator : AbstractValidator<PatientQueryParameters>
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "FirstName",
        "LastName",
        "BirthDate"
    };

    public PatientQueryParametersValidator()
    {
        Include(new PaginationQueryParametersValidator<PatientQueryParameters>());

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrEmpty(x) || AllowedSortFields.Contains(x))
            .WithMessage($"SortBy must be one of the following: {string.Join(", ", AllowedSortFields)}");
    }
}
