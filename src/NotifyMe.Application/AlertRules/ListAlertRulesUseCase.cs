using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Events;

namespace NotifyMe.Application.AlertRules;

/// <summary>
/// Lists alert rules, optionally filtered by category and/or enabled state (per
/// docs/api/admin-api.md). Filtering happens in-memory over <see cref="IAlertRuleRepository.ListAsync"/>
/// rather than adding bespoke query methods to the repository - the expected data volume for
/// this project doesn't justify a more elaborate query story.
/// </summary>
public sealed class ListAlertRulesUseCase
{
    private readonly IAlertRuleRepository _alertRuleRepository;

    public ListAlertRulesUseCase(IAlertRuleRepository alertRuleRepository)
    {
        _alertRuleRepository = alertRuleRepository;
    }

    public async Task<IReadOnlyList<AlertRuleDto>> ExecuteAsync(
        EventCategory? category, bool? isEnabled, CancellationToken cancellationToken, Guid? ownerUserId = null)
    {
        var alertRules = await _alertRuleRepository.ListAsync(cancellationToken);

        return alertRules
            .Where(rule => category is null || rule.Category == category)
            .Where(rule => isEnabled is null || rule.IsEnabled == isEnabled)
            .Where(rule => ownerUserId is null || rule.OwnerUserId == ownerUserId)
            .Select(AlertRuleDto.FromEntity)
            .ToList();
    }
}
