using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace InnoClinic.Contracts.Grpc.Auth;

// Auth0 limits machine-to-machine tokens per month, so each service keeps one cached token.
internal sealed class Auth0ServiceTokenProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<ServiceTokenOptions> options,
    TimeProvider timeProvider) : IServiceTokenProvider
{
    public const string HttpClientName = "auth0-service-token";

    private readonly SemaphoreSlim _lock = new(1, 1);
    private CachedToken? _cached;

    public async ValueTask<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        if (TryGetCached(out var token))
            return token;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (TryGetCached(out token))
                return token;

            var httpClient = httpClientFactory.CreateClient(HttpClientName);
            using var response = await httpClient.PostAsJsonAsync(
                $"https://{settings.Domain}/oauth/token",
                new TokenRequest("client_credentials", settings.ClientId, settings.ClientSecret, settings.Audience),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
                ?? throw new InvalidOperationException("Auth0 returned an empty token response.");

            var lifetime = TimeSpan.FromSeconds(body.ExpiresIn);
            var refreshMargin = settings.RefreshBeforeExpiry < lifetime / 2
                ? settings.RefreshBeforeExpiry
                : lifetime / 2;

            _cached = new CachedToken(
                body.AccessToken,
                settings.Audience,
                timeProvider.GetUtcNow() + lifetime - refreshMargin);

            return body.AccessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private bool TryGetCached(out string token)
    {
        var cached = _cached;
        if (cached is not null && cached.RefreshAt > timeProvider.GetUtcNow())
        {
            token = cached.AccessToken;
            return true;
        }

        token = string.Empty;
        return false;
    }

    private sealed record CachedToken(string AccessToken, string Audience, DateTimeOffset RefreshAt);

    private sealed record TokenRequest(
        [property: JsonPropertyName("grant_type")] string GrantType,
        [property: JsonPropertyName("client_id")] string ClientId,
        [property: JsonPropertyName("client_secret")] string ClientSecret,
        [property: JsonPropertyName("audience")] string Audience);

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
