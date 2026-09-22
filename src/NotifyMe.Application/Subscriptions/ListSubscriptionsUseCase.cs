using NotifyMe.Application.Abstractions.Repositories;

namespace NotifyMe.Application.Subscriptions;

/// <summary>
/// Lists subscriptions, optionally filtered by alert rule and/or channel (per
/// docs/api/admin-api.md). Uses <see cref="ISubscriptionRepository.ListByAlertRuleIdAsync"/> as
/// the efficient path when only <c>alertRuleId</c> is given, falling back to filtering the full
/// list in-memory otherwise - consistent with the other list use cases in this project.
/// </summary>
public sealed class ListSubscriptionsUseCase
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public ListSubscriptionsUseCase(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<IReadOnlyList<SubscriptionDto>> ExecuteAsync(
        Guid? alertRuleId, Guid? channelConfigId, CancellationToken cancellationToken, Guid? ownerUserId = null)
    {
        var subscriptions = alertRuleId is { } id
            ? await _subscriptionRepository.ListByAlertRuleIdAsync(id, cancellationToken)
            : await _subscriptionRepository.ListAsync(cancellationToken);

        return subscriptions
            .Where(subscription => channelConfigId is null || subscription.ChannelConfigId == channelConfigId)
            .Where(subscription => ownerUserId is null || subscription.OwnerUserId == ownerUserId)
            .Select(SubscriptionDto.FromEntity)
            .ToList();
    }
}
