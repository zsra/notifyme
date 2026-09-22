using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Events;

namespace NotifyMe.Domain.Abstractions;

/// <summary>
/// Finds which of a set of candidate rules should fire for a given event. A default
/// implementation built on <see cref="AlertRule.Matches"/> is added in Phase 04 alongside the
/// rest of the Application-layer use cases that consume it.
/// </summary>
public interface IAlertMatcher
{
    IReadOnlyList<AlertRule> FindMatchingRules(NormalizedEvent normalizedEvent, IReadOnlyCollection<AlertRule> candidateRules);
}
