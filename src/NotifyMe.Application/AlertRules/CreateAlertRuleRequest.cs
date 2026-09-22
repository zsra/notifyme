using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;

namespace NotifyMe.Application.AlertRules;

public sealed record CreateAlertRuleRequest(
    string Name,
    EventCategory Category,
    IReadOnlyCollection<string>? Keywords,
    Severity MinimumSeverity,
    bool IsEnabled = true);
