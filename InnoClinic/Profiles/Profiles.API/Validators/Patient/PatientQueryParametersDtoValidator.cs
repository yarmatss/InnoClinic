using FluentValidation;
using Profiles.API.DTOs.Patient;
using Profiles.API.Validators.Common;

namespace Profiles.API.Validators.Patient;

public class PatientQueryParametersDtoValidator : AbstractValidator<PatientQueryParametersDto>
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "FirstName",
        "LastName",
        "BirthDate"
    };

    public PatientQueryParametersDtoValidator()
    {
        Include(new PaginationQueryParametersValidator<PatientQueryParametersDto>());

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrEmpty(x) || AllowedSortFields.Contains(x))
            .WithMessage($"SortBy must be one of the following: {string.Join(", ", AllowedSortFields)}");
    }
}
