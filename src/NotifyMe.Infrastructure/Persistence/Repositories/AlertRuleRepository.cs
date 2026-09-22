using Microsoft.EntityFrameworkCore;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Events;

namespace NotifyMe.Infrastructure.Persistence.Repositories;

public sealed class AlertRuleRepository : IAlertRuleRepository
{
    private readonly NotifyMeDbContext _dbContext;

    public AlertRuleRepository(NotifyMeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.AlertRules.FirstOrDefaultAsync(rule => rule.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AlertRule>> ListAsync(CancellationToken cancellationToken) =>
        await _dbContext.AlertRules.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AlertRule>> ListEnabledByCategoryAsync(
        EventCategory category, CancellationToken cancellationToken) =>
        await _dbContext.AlertRules
            .AsNoTracking()
            .Where(rule => rule.IsEnabled && rule.Category == category)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(AlertRule alertRule, CancellationToken cancellationToken)
    {
        await _dbContext.AlertRules.AddAsync(alertRule, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AlertRule alertRule, CancellationToken cancellationToken)
    {
        _dbContext.AlertRules.Update(alertRule);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(AlertRule alertRule, CancellationToken cancellationToken)
    {
        _dbContext.AlertRules.Remove(alertRule);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
