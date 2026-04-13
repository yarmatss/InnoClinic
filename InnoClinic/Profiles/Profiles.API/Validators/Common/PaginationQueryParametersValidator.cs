using FluentValidation;
using Profiles.API.DTOs.Common;

namespace Profiles.API.Validators.Common;

public class PaginationQueryParametersValidator<T> : AbstractValidator<T> where T : PaginationQueryParametersDto
{
    public PaginationQueryParametersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.SortOrder)
            .Must(x => string.IsNullOrEmpty(x) ||
                       x.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                       x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortOrder must be 'asc' or 'desc'.");
    }
}
