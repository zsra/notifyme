using NotifyMe.Application.Abstractions.Repositories;

namespace NotifyMe.Application.Channels;

/// <summary>
/// Lists channels, optionally filtered by channel type and/or enabled state (per
/// docs/api/admin-api.md). See <see cref="AlertRules.ListAlertRulesUseCase"/> for the same
/// in-memory-filtering rationale.
/// </summary>
public sealed class ListChannelConfigsUseCase
{
    private readonly IChannelConfigRepository _channelConfigRepository;

    public ListChannelConfigsUseCase(IChannelConfigRepository channelConfigRepository)
    {
        _channelConfigRepository = channelConfigRepository;
    }

    public async Task<IReadOnlyList<ChannelConfigDto>> ExecuteAsync(
        string? channelType, bool? isEnabled, CancellationToken cancellationToken)
    {
        var channelConfigs = await _channelConfigRepository.ListAsync(cancellationToken);

        return channelConfigs
            .Where(channel => channelType is null || string.Equals(channel.ChannelType, channelType, StringComparison.OrdinalIgnoreCase))
            .Where(channel => isEnabled is null || channel.IsEnabled == isEnabled)
            .Select(ChannelConfigDto.FromEntity)
            .ToList();
    }
}
