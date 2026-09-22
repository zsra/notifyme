using NotifyMe.Domain.Common;

namespace NotifyMe.Domain.Alerts;

/// <summary>
/// The "does this event actually matter" part of an <see cref="AlertRule"/>, separate from
/// which <see cref="Events.EventCategory"/> it applies to. Keeping this as its own value object
/// (rather than loose fields on <see cref="AlertRule"/>) lets us enforce the rule that a
/// criterion has to be a real filter, not a no-op that matches every event in its category.
/// </summary>
public sealed class MatchCriteria
{
    public IReadOnlyCollection<string> Keywords { get; }
    public Severity MinimumSeverity { get; }

    private MatchCriteria(IReadOnlyCollection<string> keywords, Severity minimumSeverity)
    {
        Keywords = keywords;
        MinimumSeverity = minimumSeverity;
    }

    public static MatchCriteria Create(IEnumerable<string>? keywords, Severity minimumSeverity)
    {
        if (!Enum.IsDefined(typeof(Severity), minimumSeverity))
        {
            throw new ArgumentOutOfRangeException(nameof(minimumSeverity), minimumSeverity, "Unknown severity.");
        }

        var normalizedKeywords = (keywords ?? Enumerable.Empty<string>())
            .Where(keyword => !string.IsNullOrWhiteSpace(keyword))
            .Select(keyword => keyword.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedKeywords.Length == 0 && minimumSeverity == Severity.Low)
        {
            throw new ArgumentException(
                "An alert rule needs at least one real match criterion: one or more keywords, or a " +
                "minimum severity above Low. Otherwise it would match every event in its category.");
        }

        return new MatchCriteria(normalizedKeywords, minimumSeverity);
    }

    public bool IsSatisfiedBy(string title, string description, Severity eventSeverity)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(description);

        if (eventSeverity < MinimumSeverity)
        {
            return false;
        }

        if (Keywords.Count == 0)
        {
            return true;
        }

        return Keywords.Any(keyword =>
            title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
