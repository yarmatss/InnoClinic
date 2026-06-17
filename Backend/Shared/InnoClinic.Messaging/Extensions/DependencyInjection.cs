using InnoClinic.Messaging.Constants;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace InnoClinic.Messaging.Extensions;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddOutboxResilience()
        {
            services.AddResiliencePipeline(MessagingConstants.OutboxResiliencePipeline, builder =>
            {
                builder.AddTimeout(TimeSpan.FromSeconds(5));

                builder.AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential
                });
            });

            return services;
        }
    }
}
