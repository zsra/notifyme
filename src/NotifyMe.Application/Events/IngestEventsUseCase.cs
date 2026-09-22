using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Alerts;
using NotifyMe.Application.Notifications;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;

namespace NotifyMe.Application.Events;

/// <summary>
/// The end-to-end ingestion pipeline: fetch raw events from the (currently simulated)
/// <see cref="IEventSource"/>, normalize each one, evaluate alert rules against it, and dispatch
/// a notification for every (matching rule, subscribed channel) pair.
///
/// Raw/normalized events are intentionally not persisted anywhere in this phase - only
/// <see cref="Domain.Notifications.Notification"/> records are, since that's what the Admin
/// API's history endpoint actually needs to query (see docs/architecture/data-model.md). If a
/// future phase needs an event audit trail, add the repository then rather than speculatively
/// now.
/// </summary>
public sealed class IngestEventsUseCase
{
    private readonly IEventSource _eventSource;
    private readonly EvaluateAlertRulesService _evaluateAlertRulesService;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly DispatchNotificationUseCase _dispatchNotificationUseCase;

    public IngestEventsUseCase(
        IEventSource eventSource,
        EvaluateAlertRulesService evaluateAlertRulesService,
        ISubscriptionRepository subscriptionRepository,
        DispatchNotificationUseCase dispatchNotificationUseCase)
    {
        _eventSource = eventSource;
        _evaluateAlertRulesService = evaluateAlertRulesService;
        _subscriptionRepository = subscriptionRepository;
        _dispatchNotificationUseCase = dispatchNotificationUseCase;
    }

    public async Task<IngestEventsResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        var rawEvents = await _eventSource.FetchAsync(cancellationToken);
        var normalizedEvents = rawEvents.Select(Normalize).ToList();

        var dispatchedCount = 0;
        foreach (var normalizedEvent in normalizedEvents)
        {
            var matchingRules = await _evaluateAlertRulesService.EvaluateAsync(normalizedEvent, cancellationToken);

            foreach (var rule in matchingRules)
            {
                var subscriptions = await _subscriptionRepository.ListByAlertRuleIdAsync(rule.Id, cancellationToken);

                foreach (var subscription in subscriptions)
                {
                    await _dispatchNotificationUseCase.ExecuteAsync(
                        rule, normalizedEvent, subscription.ChannelConfigId, cancellationToken);
                    dispatchedCount++;
                }
            }
        }

        return new IngestEventsResult(rawEvents.Count, normalizedEvents.Count, dispatchedCount);
    }

    /// <summary>
    /// Minimal normalization for now: the raw payload becomes the title, description is empty,
    /// and severity defaults to <see cref="Severity.Medium"/>. This is expected to change once
    /// Phase 06 defines a real `SimulatedEventSource` payload shape to parse.
    /// </summary>
    private static NormalizedEvent Normalize(RawEvent rawEvent) => NormalizedEvent.Create(
        Guid.NewGuid(),
        rawEvent.Id,
        rawEvent.Category,
        title: rawEvent.Payload,
        description: null,
        severity: Severity.Medium,
        occurredAt: rawEvent.OccurredAt);
}
