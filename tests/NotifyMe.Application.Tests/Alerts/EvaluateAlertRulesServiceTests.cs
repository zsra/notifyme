using NotifyMe.Application.Alerts;
using NotifyMe.Application.Tests.TestDoubles;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using Xunit;

namespace NotifyMe.Application.Tests.Alerts;

public class EvaluateAlertRulesServiceTests
{
    private static NormalizedEvent MakeEvent(EventCategory category, string title, Severity severity) =>
        NormalizedEvent.Create(Guid.NewGuid(), Guid.NewGuid(), category, title, null, severity, DateTimeOffset.UtcNow);

    private static AlertRule MakeRule(EventCategory category, MatchCriteria criteria, bool isEnabled = true) =>
        AlertRule.Create(Guid.NewGuid(), "Rule", category, criteria, DateTimeOffset.UtcNow, isEnabled);

    [Fact]
    public async Task EvaluateAsync_MatchingKeywordAndCategory_ReturnsRule()
    {
        var repository = new InMemoryAlertRuleRepository();
        var criteria = MatchCriteria.Create(new[] { "earthquake" }, Severity.Low);
        var rule = MakeRule(EventCategory.NaturalDisaster, criteria);
        repository.Seed(rule);

        var service = new EvaluateAlertRulesService(repository, new AlertMatcher());
        var normalizedEvent = MakeEvent(EventCategory.NaturalDisaster, "Major earthquake reported", Severity.Medium);

        var matches = await service.EvaluateAsync(normalizedEvent, CancellationToken.None);

        Assert.Single(matches);
        Assert.Same(rule, matches[0]);
    }

    [Fact]
    public async Task EvaluateAsync_DifferentCategory_ReturnsNoMatches()
    {
        var repository = new InMemoryAlertRuleRepository();
        var criteria = MatchCriteria.Create(keywords: null, Severity.Medium);
        repository.Seed(MakeRule(EventCategory.MarketMovement, criteria));

        var service = new EvaluateAlertRulesService(repository, new AlertMatcher());
        var normalizedEvent = MakeEvent(EventCategory.BreakingNews, "Some title", Severity.Critical);

        var matches = await service.EvaluateAsync(normalizedEvent, CancellationToken.None);

        Assert.Empty(matches);
    }

    [Fact]
    public async Task EvaluateAsync_DisabledRule_IsExcluded()
    {
        var repository = new InMemoryAlertRuleRepository();
        var criteria = MatchCriteria.Create(keywords: null, Severity.Medium);
        repository.Seed(MakeRule(EventCategory.BreakingNews, criteria, isEnabled: false));

        var service = new EvaluateAlertRulesService(repository, new AlertMatcher());
        var normalizedEvent = MakeEvent(EventCategory.BreakingNews, "Anything", Severity.Critical);

        var matches = await service.EvaluateAsync(normalizedEvent, CancellationToken.None);

        Assert.Empty(matches);
    }

    [Fact]
    public async Task EvaluateAsync_SeverityBelowThreshold_IsExcluded()
    {
        var repository = new InMemoryAlertRuleRepository();
        var criteria = MatchCriteria.Create(keywords: null, Severity.High);
        repository.Seed(MakeRule(EventCategory.BreakingNews, criteria));

        var service = new EvaluateAlertRulesService(repository, new AlertMatcher());
        var normalizedEvent = MakeEvent(EventCategory.BreakingNews, "Anything", Severity.Low);

        var matches = await service.EvaluateAsync(normalizedEvent, CancellationToken.None);

        Assert.Empty(matches);
    }

    [Fact]
    public async Task EvaluateAsync_MultipleMatchingRules_ReturnsAllOfThem()
    {
        var repository = new InMemoryAlertRuleRepository();
        var criteria = MatchCriteria.Create(keywords: null, Severity.Medium);
        var ruleA = MakeRule(EventCategory.MarketMovement, criteria);
        var ruleB = MakeRule(EventCategory.MarketMovement, criteria);
        repository.Seed(ruleA, ruleB);

        var service = new EvaluateAlertRulesService(repository, new AlertMatcher());
        var normalizedEvent = MakeEvent(EventCategory.MarketMovement, "Stocks drop sharply", Severity.High);

        var matches = await service.EvaluateAsync(normalizedEvent, CancellationToken.None);

        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public async Task EvaluateAsync_NoCandidateRules_ReturnsEmpty()
    {
        var repository = new InMemoryAlertRuleRepository();
        var service = new EvaluateAlertRulesService(repository, new AlertMatcher());
        var normalizedEvent = MakeEvent(EventCategory.NaturalDisaster, "Anything", Severity.Critical);

        var matches = await service.EvaluateAsync(normalizedEvent, CancellationToken.None);

        Assert.Empty(matches);
    }
}
