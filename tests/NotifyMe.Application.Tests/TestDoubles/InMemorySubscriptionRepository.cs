using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Subscriptions;

namespace NotifyMe.Application.Tests.TestDoubles;

public sealed class InMemorySubscriptionRepository : ISubscriptionRepository
{
    private readonly Dictionary<Guid, Subscription> _subscriptions = new();

    public void Seed(params Subscription[] subscriptions)
    {
        foreach (var subscription in subscriptions)
        {
            _subscriptions[subscription.Id] = subscription;
        }
    }

    public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_subscriptions.GetValueOrDefault(id));

    public Task<IReadOnlyList<Subscription>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Subscription>>(_subscriptions.Values.ToList());

    public Task<IReadOnlyList<Subscription>> ListByAlertRuleIdAsync(Guid alertRuleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Subscription>>(
            _subscriptions.Values.Where(subscription => subscription.AlertRuleId == alertRuleId).ToList());

    public Task AddAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        _subscriptions[subscription.Id] = subscription;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        _subscriptions.Remove(subscription.Id);
        return Task.CompletedTask;
    }
}
