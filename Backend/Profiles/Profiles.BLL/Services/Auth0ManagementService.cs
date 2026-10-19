using Auth0.Core.Exceptions;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Users;
using InnoClinic.Core.Authorization;
using InnoClinic.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profiles.BLL.Errors;
using Profiles.BLL.Extensions;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.BLL.Options;
using System.Net;
using System.Security.Cryptography;

namespace Profiles.BLL.Services;

public class Auth0ManagementService(
    Lazy<IManagementApiClient> client,
    IOptions<Auth0ManagementOptions> options,
    ILogger<Auth0ManagementService> logger) : IAuthManagementService
{
    private readonly Auth0ManagementOptions _options = options.Value;

    public async Task<Result<UserProvisionResult>> ProvisionUserAsync(
        string email,
        string firstName,
        string lastName,
        UserRole role,
        Guid profileId,
        CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
        {
            logger.LogError("Auth0 Management API is not configured; cannot provision an account for profile {ProfileId}.", profileId);
            return AccountErrors.ProvisioningFailed;
        }

        var roleId = ResolveRoleId(role);
        if (string.IsNullOrWhiteSpace(roleId))
        {
            logger.LogError("Auth0 role ID for {Role} is not configured.", role);
            return AccountErrors.ProvisioningFailed;
        }

        string userId;
        var targetConnection = _options.Connection ?? "Username-Password-Authentication";
        try
        {
            var user = await client.Value.Users.CreateAsync(new CreateUserRequestContent
            {
                Email = email,
                GivenName = firstName,
                FamilyName = lastName,
                Name = $"{firstName} {lastName}".Trim(),
                Connection = targetConnection,
                Password = GenerateSecurePassword(),
                EmailVerified = true,
                VerifyEmail = false,
                AppMetadata = new Dictionary<string, object?>
                {
                    ["profile_id"] = profileId.ToString(),
                    ["role"] = role.ToString()
                }
            }, cancellationToken: ct);

            userId = user.UserId ?? throw new InvalidOperationException("Auth0 returned a user without an ID.");
        }
        catch (Exception ex) when (IsConflict(ex))
        {
            logger.LogWarning(ex, "An Auth0 account for profile {ProfileId} already exists; refusing to reuse it.", profileId);
            return AccountErrors.EmailAlreadyRegistered;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create Auth0 user for profile {ProfileId}.", profileId);
            return AccountErrors.ProvisioningFailed;
        }

        try
        {
            await client.Value.Users.Roles.AssignAsync(userId, new AssignUserRolesRequestContent
            {
                Roles = [roleId]
            }, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to assign role {Role} to Auth0 user {UserId}; removing the user.", role, userId);
            await DeleteUserAsync(userId, CancellationToken.None);
            return AccountErrors.ProvisioningFailed;
        }

        var ticketUrl = await GenerateInvitationTicketAsync(userId, ct);

        return new UserProvisionResult(userId, ticketUrl);
    }

    public async Task<Result> ChangeRoleAsync(
        string userId,
        UserRole currentRole,
        UserRole newRole,
        CancellationToken ct = default)
    {
        var currentRoleId = ResolveRoleId(currentRole);
        var newRoleId = ResolveRoleId(newRole);
        if (!_options.IsConfigured || string.IsNullOrWhiteSpace(currentRoleId) || string.IsNullOrWhiteSpace(newRoleId))
        {
            logger.LogError("Auth0 Management API or role IDs are not configured; cannot change the role of {UserId}.", userId);
            return AccountErrors.UpdateFailed;
        }

        try
        {
            await client.Value.Users.Roles.AssignAsync(userId, new AssignUserRolesRequestContent
            {
                Roles = [newRoleId]
            }, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to assign role {Role} to Auth0 user {UserId}.", newRole, userId);
            return AccountErrors.UpdateFailed;
        }

        try
        {
            await client.Value.Users.Roles.DeleteAsync(userId, new DeleteUserRolesRequestContent
            {
                Roles = [currentRoleId]
            }, cancellationToken: ct);

            await client.Value.Users.UpdateAsync(userId, new UpdateUserRequestContent
            {
                AppMetadata = new Dictionary<string, object?> { ["role"] = newRole.ToString() }
            }, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to change the role of Auth0 user {UserId} to {Role}; reverting.", userId, newRole);
            await RevertRoleAsync(userId, currentRoleId, newRoleId);
            return AccountErrors.UpdateFailed;
        }

        return Result.Success();
    }

    public async Task<Result> SetBlockedAsync(string userId, bool blocked, CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
        {
            logger.LogError("Auth0 Management API is not configured; cannot update {UserId}.", userId);
            return AccountErrors.UpdateFailed;
        }

        try
        {
            await client.Value.Users.UpdateAsync(userId, new UpdateUserRequestContent
            {
                Blocked = blocked
            }, cancellationToken: ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set blocked = {Blocked} for Auth0 user {UserId}.", blocked, userId);
            return AccountErrors.UpdateFailed;
        }
    }

    public async Task DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            await client.Value.Users.DeleteAsync(userId, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete Auth0 user {UserId}; it must be removed manually.", userId);
        }
    }

    private async Task RevertRoleAsync(string userId, string previousRoleId, string attemptedRoleId)
    {
        try
        {
            await client.Value.Users.Roles.AssignAsync(userId, new AssignUserRolesRequestContent
            {
                Roles = [previousRoleId]
            }, cancellationToken: CancellationToken.None);

            await client.Value.Users.Roles.DeleteAsync(userId, new DeleteUserRolesRequestContent
            {
                Roles = [attemptedRoleId]
            }, cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogRoleRevertFailed(ex, userId);
        }
    }

    private async Task<string?> GenerateInvitationTicketAsync(string userId, CancellationToken ct)
    {
        try
        {
            var ticket = await client.Value.Tickets.ChangePasswordAsync(new ChangePasswordTicketRequestContent
            {
                UserId = userId,
                ResultUrl = _options.InvitationRedirectUrl,
                TtlSec = _options.InvitationTicketTtlSec
            }, cancellationToken: ct);

            return ticket.Ticket;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create password ticket for user {UserId}", userId);
            return null;
        }
    }

    private static bool IsConflict(Exception ex) =>
        ex is ConflictError || (ex is ErrorApiException apiEx && apiEx.StatusCode == HttpStatusCode.Conflict);

    private string? ResolveRoleId(UserRole role)
    {
        return role switch
        {
            UserRole.Patient => _options.PatientRoleId,
            UserRole.Doctor => _options.DoctorRoleId,
            UserRole.Administrator => _options.AdministratorRoleId,
            UserRole.Receptionist => _options.ReceptionistRoleId,
            _ => null
        };
    }

    private static string GenerateSecurePassword()
    {
        const string upper = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";

        var bytes = RandomNumberGenerator.GetBytes(16);
        var chars = new char[16];
        chars[0] = upper[bytes[0] % upper.Length];
        chars[1] = lower[bytes[1] % lower.Length];
        chars[2] = digits[bytes[2] % digits.Length];
        chars[3] = special[bytes[3] % special.Length];

        for (int i = 4; i < 16; i++)
        {
            chars[i] = (upper + lower + digits + special)[bytes[i] % (upper.Length + lower.Length + digits.Length + special.Length)];
        }

        return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(100)).ToArray());
    }
}
