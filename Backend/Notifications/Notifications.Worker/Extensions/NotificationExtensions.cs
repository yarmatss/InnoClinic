using Notifications.Worker.Constants;
using Notifications.Worker.Interfaces;
using Notifications.Worker.Options;
using Notifications.Worker.Services;
using Polly;

namespace Notifications.Worker.Extensions;

public static class NotificationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddNotifications(IConfiguration configuration)
        {
            services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
            
            services.AddSingleton<IEmailSenderService, EmailSenderService>();

            services.AddResiliencePipeline(NotificationConstants.EmailRetryPipeline, builder =>
            {
                builder.AddRetry(new()
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                });
            });

            return services;
        }
    }
}
