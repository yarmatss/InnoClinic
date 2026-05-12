using Profiles.Domain.Constants;

namespace Profiles.Domain.Common;

public record ValidationError : Error
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationError(IDictionary<string, string[]> errors)
        : base(ValidationConstants.ValidationFailed, ValidationConstants.ValidationFailedTitle, ErrorType.Validation)
    {
        Errors = errors;
    }
}
