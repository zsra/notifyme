using Microsoft.EntityFrameworkCore;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Subscriptions;

namespace NotifyMe.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly NotifyMeDbContext _dbContext;

    public SubscriptionRepository(NotifyMeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Subscriptions.FirstOrDefaultAsync(subscription => subscription.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Subscription>> ListAsync(CancellationToken cancellationToken) =>
        await _dbContext.Subscriptions.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Subscription>> ListByAlertRuleIdAsync(
        Guid alertRuleId, CancellationToken cancellationToken) =>
        await _dbContext.Subscriptions
            .AsNoTracking()
            .Where(subscription => subscription.AlertRuleId == alertRuleId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        await _dbContext.Subscriptions.AddAsync(subscription, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        _dbContext.Subscriptions.Remove(subscription);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
