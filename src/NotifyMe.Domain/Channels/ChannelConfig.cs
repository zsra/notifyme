using NotifyMe.Domain.Common;

namespace NotifyMe.Domain.Channels;

/// <summary>
/// Where a <see cref="Subscriptions.Subscription"/> can deliver a notification to. Deliberately
/// keyed by a free-form <see cref="ChannelType"/> string rather than a closed enum, so a new
/// channel implementation is "add one class + one DI registration" - see
/// docs/architecture/notification-channels.md for the full reasoning.
/// </summary>
public sealed class ChannelConfig : Entity
{
    public string ChannelType { get; private set; } = string.Empty;
    public string Target { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }

    private ChannelConfig()
    {
    }

    private ChannelConfig(Guid id, string channelType, string target, bool isEnabled)
        : base(id)
    {
        ChannelType = channelType;
        Target = target;
        IsEnabled = isEnabled;
    }

    public static ChannelConfig Create(Guid id, string channelType, string target, bool isEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(channelType))
        {
            throw new ArgumentException("Channel type is required.", nameof(channelType));
        }

        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ArgumentException(
                "Channel target (e.g. a webhook URL or an email address) is required.", nameof(target));
        }

        return new ChannelConfig(id, channelType.Trim().ToLowerInvariant(), target.Trim(), isEnabled);
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;
}
