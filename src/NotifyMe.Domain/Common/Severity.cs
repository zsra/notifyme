namespace NotifyMe.Domain.Common;

/// <summary>
/// How severe a normalized event is judged to be, and the threshold an <see cref="Alerts.AlertRule"/>
/// can require. Ordered low to high so callers can compare with relational operators.
/// </summary>
public enum Severity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3,
}
