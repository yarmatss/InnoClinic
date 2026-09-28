using System.Net;
using System.Security.Cryptography;
using Auth0.Core.Exceptions;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Users;
using InnoClinic.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.BLL.Options;

namespace Profiles.BLL.Services;

public class Auth0ManagementService(
    IOptions<Auth0ManagementOptions> options,
    ILogger<Auth0ManagementService> logger,
    IManagementApiClient? client = null) : IAuth0ManagementService
{
    private readonly Auth0ManagementOptions _options = options.Value;

    public async Task<Result<Auth0UserProvisionResult>> ProvisionUserAsync(
        string email,
        string firstName,
        string lastName,
        string roleName,
        CancellationToken ct = default)
    {
        if (!_options.IsConfigured || client is null)
        {
            logger.LogWarning("Auth0 Management API is not configured. Falling back to local placeholder user ID.");
            var fallbackId = $"auth0|local_{Guid.NewGuid():N}";
            return new Auth0UserProvisionResult(fallbackId, null);
        }

        try
        {
            var userId = await CreateOrGetUserIdAsync(client, email, firstName, lastName, ct);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return new Error("Auth0.UserCreationFailure", "Could not obtain user ID from Auth0.", ErrorType.Failure);
            }

            var roleId = ResolveRoleId(roleName);
            if (!string.IsNullOrWhiteSpace(roleId))
            {
                await AssignRoleAsync(client, userId, roleId, ct);
            }

            var ticketUrl = await GenerateInvitationTicketAsync(client, userId, ct);

            return new Auth0UserProvisionResult(userId, ticketUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to provision Auth0 user for email {Email}", email);
            return new Error("Auth0.ProvisionError", ex.Message, ErrorType.Failure);
        }
    }

    private async Task<string?> CreateOrGetUserIdAsync(
        IManagementApiClient client,
        string email,
        string firstName,
        string lastName,
        CancellationToken ct)
    {
        try
        {
            var user = await client.Users.CreateAsync(new CreateUserRequestContent
            {
                Email = email,
                GivenName = firstName,
                FamilyName = lastName,
                Name = $"{firstName} {lastName}".Trim(),
                Connection = _options.Connection ?? "Username-Password-Authentication",
                Password = GenerateSecurePassword(),
                EmailVerified = true,
                VerifyEmail = false
            }, cancellationToken: ct);

            return user.UserId;
        }
        catch (ErrorApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            logger.LogInformation("User with email {Email} already exists in Auth0. Retrieving existing user.", email);
            var users = await client.Users.ListUsersByEmailAsync(new ListUsersByEmailRequestParameters
            {
                Email = email
            }, cancellationToken: ct);

            return users.FirstOrDefault()?.UserId;
        }
    }

    private async Task AssignRoleAsync(IManagementApiClient client, string userId, string roleId, CancellationToken ct)
    {
        try
        {
            await client.Users.Roles.AssignAsync(userId, new AssignUserRolesRequestContent
            {
                Roles = [roleId]
            }, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to assign role {RoleId} to user {UserId}", roleId, userId);
        }
    }

    private async Task<string?> GenerateInvitationTicketAsync(IManagementApiClient client, string userId, CancellationToken ct)
    {
        try
        {
            var ticket = await client.Tickets.ChangePasswordAsync(new ChangePasswordTicketRequestContent
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

    private string? ResolveRoleId(string roleName)
    {
        return roleName.ToLowerInvariant() switch
        {
            "patient" => _options.PatientRoleId,
            "doctor" => _options.DoctorRoleId,
            "administrator" => _options.AdministratorRoleId,
            "receptionist" => _options.ReceptionistRoleId,
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
