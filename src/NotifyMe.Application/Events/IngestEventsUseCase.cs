using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
///
/// Each call is tagged with a fresh "ingestion run" correlation id, and each normalized event
/// gets its own correlation id (its <see cref="NormalizedEvent.Id"/>, the same id persisted on
/// every resulting <see cref="Domain.Notifications.Notification.NormalizedEventId"/>) so log
/// lines emitted here and in <see cref="DispatchNotificationUseCase"/> during the same call can
/// be traced back to the same event and, from there, to the database (see
/// ai/plan/phase-09-background-workers.md).
/// </summary>
public sealed class IngestEventsUseCase
{
    private readonly IEventSource _eventSource;
    private readonly EvaluateAlertRulesService _evaluateAlertRulesService;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly DispatchNotificationUseCase _dispatchNotificationUseCase;
    private readonly ILogger<IngestEventsUseCase> _logger;

    public IngestEventsUseCase(
        IEventSource eventSource,
        EvaluateAlertRulesService evaluateAlertRulesService,
        ISubscriptionRepository subscriptionRepository,
        DispatchNotificationUseCase dispatchNotificationUseCase,
        ILogger<IngestEventsUseCase>? logger = null)
    {
        _eventSource = eventSource;
        _evaluateAlertRulesService = evaluateAlertRulesService;
        _subscriptionRepository = subscriptionRepository;
        _dispatchNotificationUseCase = dispatchNotificationUseCase;
        _logger = logger ?? NullLogger<IngestEventsUseCase>.Instance;
    }

    public async Task<IngestEventsResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        var ingestionRunId = Guid.NewGuid();
        using var runScope = _logger.BeginScope(new Dictionary<string, object> { ["IngestionRunId"] = ingestionRunId });

        var rawEvents = await _eventSource.FetchAsync(cancellationToken);
        var normalizedEvents = rawEvents.Select(Normalize).ToList();
        _logger.LogInformation("Fetched {RawEventCount} raw event(s), normalized to {NormalizedEventCount}.", rawEvents.Count, normalizedEvents.Count);

        var dispatchedCount = 0;
        foreach (var normalizedEvent in normalizedEvents)
        {
            using var eventScope = _logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = normalizedEvent.Id });

            var matchingRules = await _evaluateAlertRulesService.EvaluateAsync(normalizedEvent, cancellationToken);
            _logger.LogDebug("Event {CorrelationId} matched {MatchingRuleCount} alert rule(s).", normalizedEvent.Id, matchingRules.Count);

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

        _logger.LogInformation("Ingestion run complete: dispatched {DispatchedCount} notification(s).", dispatchedCount);
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
