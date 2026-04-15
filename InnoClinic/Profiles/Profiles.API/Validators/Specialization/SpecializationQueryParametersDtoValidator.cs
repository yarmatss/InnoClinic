using FluentValidation;
using Profiles.Domain.Models;
using Profiles.API.Validators.Common;

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
