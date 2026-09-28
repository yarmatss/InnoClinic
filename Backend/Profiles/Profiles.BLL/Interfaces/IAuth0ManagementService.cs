using InnoClinic.Core.Common;
using Profiles.BLL.Models;

namespace Profiles.BLL.Interfaces;

public interface IAuth0ManagementService
{
    Task<Result<Auth0UserProvisionResult>> ProvisionUserAsync(
        string email,
        string firstName,
        string lastName,
        string roleName,
        CancellationToken ct = default);
}
