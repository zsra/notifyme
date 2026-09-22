using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Events;

namespace NotifyMe.Application.Alerts;

/// <summary>
/// Loads the candidate rules for a <see cref="NormalizedEvent"/>'s category and runs them
/// through an <see cref="IAlertMatcher"/>. This is the piece <see cref="Events.IngestEventsUseCase"/>
/// calls per event during ingestion.
/// </summary>
public sealed class EvaluateAlertRulesService
{
    private readonly IAlertRuleRepository _alertRuleRepository;
    private readonly IAlertMatcher _alertMatcher;

    public EvaluateAlertRulesService(IAlertRuleRepository alertRuleRepository, IAlertMatcher alertMatcher)
    {
        _alertRuleRepository = alertRuleRepository;
        _alertMatcher = alertMatcher;
    }

    public async Task<IReadOnlyList<AlertRule>> EvaluateAsync(NormalizedEvent normalizedEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(normalizedEvent);

        var candidateRules = await _alertRuleRepository.ListEnabledByCategoryAsync(normalizedEvent.Category, cancellationToken);

        return _alertMatcher.FindMatchingRules(normalizedEvent, candidateRules);
    }
}
