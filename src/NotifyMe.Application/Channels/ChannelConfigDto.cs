using NotifyMe.Domain.Channels;

namespace NotifyMe.Application.Channels;

public sealed record ChannelConfigDto(Guid Id, string ChannelType, string Target, bool IsEnabled)
{
    public static ChannelConfigDto FromEntity(ChannelConfig channelConfig) => new(
        channelConfig.Id,
        channelConfig.ChannelType,
        channelConfig.Target,
        channelConfig.IsEnabled);
}
