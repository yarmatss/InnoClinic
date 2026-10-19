using InnoClinic.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Profiles.IntegrationTests.Infrastructure;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) 
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string AuthenticationScheme = "TestScheme";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Context.Request.Headers.TryGetValue("X-Test-Anonymous", out _))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClinicClaims.Subject, "test-user-id"),
            new(ClinicClaims.Role, Context.Request.Headers.TryGetValue("X-Test-Role", out var r) ? r.ToString() : "Administrator"),
            new(ClinicClaims.ProfileId, Context.Request.Headers.TryGetValue("X-Test-Profile-Id", out var pid) ? pid.ToString() : "00000000-0000-0000-0000-000000000001")
        };

        var permissions = Context.Request.Headers.TryGetValue("X-Test-Permission", out var requested)
            ? requested.Select(p => p!)
            : ClinicPermissions.All;

        claims.AddRange(permissions.Select(p => new Claim(ClinicClaims.Permissions, p)));

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}
