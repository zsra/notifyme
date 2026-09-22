using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Infrastructure.NotificationChannels.Email;
using NotifyMe.Infrastructure.NotificationChannels.Slack;

namespace NotifyMe.Infrastructure.NotificationChannels;

/// <summary>
/// Registers the notification-channel implementations against the shared
/// <see cref="INotificationChannel"/> abstraction. Adding a new channel means implementing
/// <see cref="INotificationChannel"/> and adding one more registration here - no changes to
/// `Domain`/`Application` (see docs/architecture/notification-channels.md).
/// </summary>
public static class NotificationChannelsServiceCollectionExtensions
{
    public static IServiceCollection AddNotifyMeNotificationChannels(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailChannelOptions>(configuration.GetSection(EmailChannelOptions.SectionName));
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<INotificationChannel, EmailNotificationChannel>();

        services.AddHttpClient<SlackNotificationChannel>();
        services.AddTransient<INotificationChannel>(sp => sp.GetRequiredService<SlackNotificationChannel>());

        return services;
    }
}
