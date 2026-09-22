namespace NotifyMe.Application.Common.Resilience;

/// <summary>
/// Configures the Polly retry policy wrapped around <c>INotificationChannel.SendAsync</c> calls
/// in <see cref="Notifications.DispatchNotificationUseCase"/> (see
/// ai/plan/phase-09-background-workers.md). Bound from the `Resilience:ChannelSend`
/// configuration section.
/// </summary>
public sealed class ChannelSendResilienceOptions
{
    public const string SectionName = "Resilience:ChannelSend";

    /// <summary>Number of retry attempts after the initial send (0 disables retries).</summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Base delay for exponential backoff between retries.</summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);
}
