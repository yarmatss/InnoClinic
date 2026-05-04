using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profiles.API.Authorization;

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
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new(ClaimTypes.Name, "TestUser")
        };

        if (Context.Request.Headers.TryGetValue("X-Test-Scope", out var scopes))
        {
            foreach (var scope in scopes)
            {
                claims.Add(new Claim("scope", scope!));
            }
        }
        else
        {
            claims.Add(new Claim("scope", $"{Policies.ScopeReadPatients} {Policies.ScopeWritePatients} {Policies.ScopeReadStaff} {Policies.ScopeWriteStaff} {Policies.ScopeWriteSpecializations}"));
        }

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}
