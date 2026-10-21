using FluentValidation;
using Profiles.API.Validators.Common;
using Profiles.Domain.Models;

namespace Profiles.API.Validators.Specialization;

public class SpecializationQueryParametersValidator : AbstractValidator<SpecializationQueryParameters>
{
    public SpecializationQueryParametersValidator()
    {
        Include(new PaginationQueryParametersValidator<SpecializationQueryParameters>());

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrEmpty(x) || x.Equals("Name", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortBy must be 'Name'.");
    }
}
