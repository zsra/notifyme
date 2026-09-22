namespace NotifyMe.Application.Channels;

public sealed record CreateChannelConfigRequest(string ChannelType, string Target, bool IsEnabled = true);
