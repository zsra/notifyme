using NotifyMe.Domain.Channels;

namespace NotifyMe.Application.Abstractions.Repositories;

public interface IChannelConfigRepository
{
    Task<ChannelConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ChannelConfig>> ListAsync(CancellationToken cancellationToken);

    Task AddAsync(ChannelConfig channelConfig, CancellationToken cancellationToken);

    Task UpdateAsync(ChannelConfig channelConfig, CancellationToken cancellationToken);

    Task DeleteAsync(ChannelConfig channelConfig, CancellationToken cancellationToken);
}
