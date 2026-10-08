using InnoClinic.Core.Authorization;
using InnoClinic.Core.Common;
using Profiles.BLL.Models;

namespace Profiles.BLL.Interfaces;

public interface IAuthManagementService
{
    Task<Result<UserProvisionResult>> ProvisionUserAsync(
        string email,
        string firstName,
        string lastName,
        UserRole role,
        Guid profileId,
        CancellationToken ct = default);
}
