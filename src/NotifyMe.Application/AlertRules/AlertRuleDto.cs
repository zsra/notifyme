using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;

namespace NotifyMe.Application.AlertRules;

public sealed record AlertRuleDto(
    Guid Id,
    string Name,
    EventCategory Category,
    IReadOnlyCollection<string> Keywords,
    Severity MinimumSeverity,
    bool IsEnabled,
    DateTimeOffset CreatedAt)
{
    public static AlertRuleDto FromEntity(AlertRule alertRule) => new(
        alertRule.Id,
        alertRule.Name,
        alertRule.Category,
        alertRule.Criteria.Keywords,
        alertRule.Criteria.MinimumSeverity,
        alertRule.IsEnabled,
        alertRule.CreatedAt);
}
