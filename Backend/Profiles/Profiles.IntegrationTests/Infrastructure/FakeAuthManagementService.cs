using InnoClinic.Core.Authorization;
using InnoClinic.Core.Common;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;

namespace Profiles.IntegrationTests.Infrastructure;

public sealed class FakeAuthManagementService : IAuthManagementService
{
    public Task<Result<UserProvisionResult>> ProvisionUserAsync(
        string email,
        string firstName,
        string lastName,
        UserRole role,
        Guid profileId,
        CancellationToken ct = default) =>
        Task.FromResult<Result<UserProvisionResult>>(new UserProvisionResult($"auth0|{profileId}", null));

    public Task<Result> ChangeRoleAsync(string userId, UserRole currentRole, UserRole newRole, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> SetBlockedAsync(string userId, bool blocked, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task DeleteUserAsync(string userId, CancellationToken ct = default) => Task.CompletedTask;
}
