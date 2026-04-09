namespace Profiles.Domain.Common;

public sealed record Error(string Code, string Description, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static implicit operator Result(Error error) => Result.Failure(error);
}
