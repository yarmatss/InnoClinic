using InnoClinic.Core.Common;

namespace Profiles.BLL.Errors;

public class AccountErrors : DomainErrors
{
    public static readonly Error EmailAlreadyRegistered = CreateConflict(
        "Account.EmailAlreadyRegistered",
        "An account with this email already exists.");

    public static readonly Error ProvisioningFailed = new(
        "Account.ProvisioningFailed",
        "The user account could not be created. Please try again later.",
        ErrorType.Failure);

    public static readonly Error UpdateFailed = new(
        "Account.UpdateFailed",
        "The user account could not be updated. Please try again later.",
        ErrorType.Failure);
}
