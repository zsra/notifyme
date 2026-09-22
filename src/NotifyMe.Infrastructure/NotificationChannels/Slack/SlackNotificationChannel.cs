using System.Net.Http.Json;
using System.Text.Json.Serialization;
using NotifyMe.Domain.Abstractions;

namespace NotifyMe.Infrastructure.NotificationChannels.Slack;

/// <summary>
/// Sends notifications to a Slack Incoming Webhook via a plain HTTP POST (see
/// docs/architecture/notification-channels.md and ADR-0005). No OAuth/bot token needed - the
/// webhook URL itself (stored as <see cref="Domain.Channels.ChannelConfig.Target"/>) is the
/// only "credential".
/// </summary>
public sealed class SlackNotificationChannel : INotificationChannel
{
    private readonly HttpClient _httpClient;

    public SlackNotificationChannel(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string ChannelType => "slack";

    public async Task<NotificationDispatchResult> SendAsync(NotificationDispatchContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var payload = new SlackWebhookPayload(BuildMessageText(context));

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(context.Channel.Target, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return NotificationDispatchResult.Success();
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return NotificationDispatchResult.Failure($"Slack webhook returned {(int)response.StatusCode}: {body}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return NotificationDispatchResult.Failure($"Failed to post to Slack webhook: {ex.Message}");
        }
    }

    private static string BuildMessageText(NotificationDispatchContext context)
    {
        var text = $"*{context.Rule.Name}* [{context.Event.Category}/{context.Event.Severity}]: {context.Event.Title}";
        return string.IsNullOrWhiteSpace(context.Event.Description) ? text : $"{text}\n{context.Event.Description}";
    }

    private sealed record SlackWebhookPayload([property: JsonPropertyName("text")] string Text);
}
