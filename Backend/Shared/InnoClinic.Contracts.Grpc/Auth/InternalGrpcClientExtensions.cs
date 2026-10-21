using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Security;

namespace InnoClinic.Contracts.Grpc.Auth;

public static class InternalGrpcClientExtensions
{
    public const string AllowUntrustedServerCertificateKey = "Grpc:AllowUntrustedServerCertificate";

    extension(IServiceCollection services)
    {
        public IHttpClientBuilder AddInternalGrpcClient<TClient>(string address, IConfiguration configuration)
            where TClient : ClientBase
        {
            services.AddServiceTokenProvider(configuration);

            var allowUntrustedCertificate = configuration.GetValue<bool>(AllowUntrustedServerCertificateKey);

            return services
                .AddGrpcClient<TClient>(options => options.Address = new Uri(address))
                .ConfigurePrimaryHttpMessageHandler(() => CreateSocketsHttpHandler(allowUntrustedCertificate))
                .AddCallCredentials(async (context, metadata, serviceProvider) =>
                {
                    var token = await serviceProvider
                        .GetRequiredService<IServiceTokenProvider>()
                        .GetTokenAsync(context.CancellationToken);

                    metadata.Add("Authorization", $"Bearer {token}");
                });
        }

        public IServiceCollection AddServiceTokenProvider(IConfiguration configuration)
        {
            if (services.Any(d => d.ServiceType == typeof(IServiceTokenProvider)))
                return services;

            services.AddOptions<ServiceTokenOptions>()
                .Configure(options =>
                {
                    options.Domain = configuration["Auth0:Domain"] ?? string.Empty;
                    options.Audience = configuration["Auth0:InternalAudience"] ?? string.Empty;
                    options.ClientId = configuration["Auth0:Services:ClientId"] ?? string.Empty;
                    options.ClientSecret = configuration["Auth0:Services:ClientSecret"] ?? string.Empty;
                })
                .Validate(
                    o => !string.IsNullOrWhiteSpace(o.Domain)
                        && !string.IsNullOrWhiteSpace(o.Audience)
                        && !string.IsNullOrWhiteSpace(o.ClientId)
                        && !string.IsNullOrWhiteSpace(o.ClientSecret),
                    "Auth0:Domain, Auth0:InternalAudience and Auth0:Services:ClientId/ClientSecret are required for service-to-service calls.")
                .ValidateOnStart();

            services.TryAddSingleton(TimeProvider.System);
            services.AddHttpClient(Auth0ServiceTokenProvider.HttpClientName);
            services.AddSingleton<IServiceTokenProvider, Auth0ServiceTokenProvider>();

            return services;
        }
    }

    private static SocketsHttpHandler CreateSocketsHttpHandler(bool allowUntrustedCertificate)
    {
        return new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true,
            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (_, _, _, sslPolicyErrors) =>
                    sslPolicyErrors == SslPolicyErrors.None || allowUntrustedCertificate
            }
        };
    }
}
