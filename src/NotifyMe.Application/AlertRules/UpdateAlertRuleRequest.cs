using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;

namespace NotifyMe.Application.AlertRules;

public sealed record UpdateAlertRuleRequest(
    Guid Id,
    string Name,
    EventCategory Category,
    IReadOnlyCollection<string>? Keywords,
    Severity MinimumSeverity);
