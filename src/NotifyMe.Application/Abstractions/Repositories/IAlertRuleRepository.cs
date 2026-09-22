using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Events;

namespace NotifyMe.Application.Abstractions.Repositories;

public interface IAlertRuleRepository
{
    Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<AlertRule>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// The candidate set an <see cref="Alerts.EvaluateAlertRulesService"/> matches a
    /// <see cref="NormalizedEvent"/> against: only enabled rules in the same category.
    /// </summary>
    Task<IReadOnlyList<AlertRule>> ListEnabledByCategoryAsync(EventCategory category, CancellationToken cancellationToken);

    Task AddAsync(AlertRule alertRule, CancellationToken cancellationToken);

    Task UpdateAsync(AlertRule alertRule, CancellationToken cancellationToken);

    Task DeleteAsync(AlertRule alertRule, CancellationToken cancellationToken);
}
