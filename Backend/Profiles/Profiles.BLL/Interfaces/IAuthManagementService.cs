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

    Task<Result> ChangeRoleAsync(
        string userId,
        UserRole currentRole,
        UserRole newRole,
        CancellationToken ct = default);

    Task<Result> SetBlockedAsync(
        string userId,
        bool blocked,
        CancellationToken ct = default);

    Task DeleteUserAsync(string userId, CancellationToken ct = default);
}
