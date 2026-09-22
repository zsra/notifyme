using Microsoft.EntityFrameworkCore;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Channels;

namespace NotifyMe.Infrastructure.Persistence.Repositories;

public sealed class ChannelConfigRepository : IChannelConfigRepository
{
    private readonly NotifyMeDbContext _dbContext;

    public ChannelConfigRepository(NotifyMeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ChannelConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.ChannelConfigs.FirstOrDefaultAsync(channel => channel.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ChannelConfig>> ListAsync(CancellationToken cancellationToken) =>
        await _dbContext.ChannelConfigs.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(ChannelConfig channelConfig, CancellationToken cancellationToken)
    {
        await _dbContext.ChannelConfigs.AddAsync(channelConfig, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ChannelConfig channelConfig, CancellationToken cancellationToken)
    {
        _dbContext.ChannelConfigs.Update(channelConfig);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ChannelConfig channelConfig, CancellationToken cancellationToken)
    {
        _dbContext.ChannelConfigs.Remove(channelConfig);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
