using InnoClinic.Core.Common;
using Profiles.BLL.Models;
using Profiles.Domain.Enums;

namespace Profiles.BLL.Interfaces;

public interface IAuthManagementService
{
    Task<Result<UserProvisionResult>> ProvisionUserAsync(
        string email,
        string firstName,
        string lastName,
        UserRole role,
        CancellationToken ct = default);
}
