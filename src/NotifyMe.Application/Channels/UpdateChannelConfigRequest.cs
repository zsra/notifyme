namespace NotifyMe.Application.Channels;

public sealed record UpdateChannelConfigRequest(Guid Id, string ChannelType, string Target);
