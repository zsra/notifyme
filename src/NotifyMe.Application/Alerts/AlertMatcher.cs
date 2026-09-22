using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Events;

namespace NotifyMe.Application.Alerts;

/// <summary>
/// Default <see cref="IAlertMatcher"/> implementation: an event matches a rule if
/// <see cref="AlertRule.Matches"/> says so. Kept this thin on purpose - all the actual matching
/// logic (category, enabled state, keyword/severity criteria) already lives on the domain
/// entities themselves; this class just applies it across a candidate set.
/// </summary>
public sealed class AlertMatcher : IAlertMatcher
{
    public IReadOnlyList<AlertRule> FindMatchingRules(NormalizedEvent normalizedEvent, IReadOnlyCollection<AlertRule> candidateRules)
    {
        ArgumentNullException.ThrowIfNull(normalizedEvent);
        ArgumentNullException.ThrowIfNull(candidateRules);

        return candidateRules.Where(rule => rule.Matches(normalizedEvent)).ToList();
    }
}
