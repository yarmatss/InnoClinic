using FluentValidation;
using Profiles.API.DTOs.Specialization;
using Profiles.API.Validators.Common;

namespace Profiles.API.Validators.Specialization;

public class SpecializationQueryParametersDtoValidator : AbstractValidator<SpecializationQueryParametersDto>
{
    public SpecializationQueryParametersDtoValidator()
    {
        Include(new PaginationQueryParametersValidator<SpecializationQueryParametersDto>());

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrEmpty(x) || x.Equals("Name", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortBy must be 'Name'.");
    }
}
