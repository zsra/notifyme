using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Channels;

namespace NotifyMe.Application.Tests.TestDoubles;

public sealed class InMemoryChannelConfigRepository : IChannelConfigRepository
{
    private readonly Dictionary<Guid, ChannelConfig> _channelConfigs = new();

    public void Seed(params ChannelConfig[] channelConfigs)
    {
        foreach (var channelConfig in channelConfigs)
        {
            _channelConfigs[channelConfig.Id] = channelConfig;
        }
    }

    public Task<ChannelConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_channelConfigs.GetValueOrDefault(id));

    public Task<IReadOnlyList<ChannelConfig>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ChannelConfig>>(_channelConfigs.Values.ToList());

    public Task AddAsync(ChannelConfig channelConfig, CancellationToken cancellationToken)
    {
        _channelConfigs[channelConfig.Id] = channelConfig;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ChannelConfig channelConfig, CancellationToken cancellationToken)
    {
        _channelConfigs[channelConfig.Id] = channelConfig;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ChannelConfig channelConfig, CancellationToken cancellationToken)
    {
        _channelConfigs.Remove(channelConfig.Id);
        return Task.CompletedTask;
    }
}
