using NotifyMe.Domain.Subscriptions;

namespace NotifyMe.Application.Abstractions.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Subscription>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// The channels a matched <see cref="Domain.Alerts.AlertRule"/> should notify through.
    /// </summary>
    Task<IReadOnlyList<Subscription>> ListByAlertRuleIdAsync(Guid alertRuleId, CancellationToken cancellationToken);

    Task AddAsync(Subscription subscription, CancellationToken cancellationToken);

    Task DeleteAsync(Subscription subscription, CancellationToken cancellationToken);
}
