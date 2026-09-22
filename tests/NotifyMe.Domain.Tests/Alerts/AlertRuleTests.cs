using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using Xunit;

namespace NotifyMe.Domain.Tests.Alerts;

public class AlertRuleTests
{
    private static MatchCriteria SeverityOnlyCriteria(Severity minimumSeverity) =>
        MatchCriteria.Create(keywords: null, minimumSeverity);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_Throws(string? name)
    {
        var act = () => AlertRule.Create(
            Guid.NewGuid(), name!, EventCategory.BreakingNews, SeverityOnlyCriteria(Severity.Medium), DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithUndefinedCategory_Throws()
    {
        var act = () => AlertRule.Create(
            Guid.NewGuid(), "Rule", (EventCategory)999, SeverityOnlyCriteria(Severity.Medium), DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Create_DefaultsToEnabled()
    {
        var rule = AlertRule.Create(
            Guid.NewGuid(), "Rule", EventCategory.BreakingNews, SeverityOnlyCriteria(Severity.Medium), DateTimeOffset.UtcNow);

        Assert.True(rule.IsEnabled);
    }

    [Fact]
    public void Matches_DisabledRule_NeverMatches()
    {
        var rule = AlertRule.Create(
            Guid.NewGuid(), "Rule", EventCategory.BreakingNews, SeverityOnlyCriteria(Severity.Medium), DateTimeOffset.UtcNow);
        rule.Disable();

        var normalizedEvent = NormalizedEvent.Create(
            Guid.NewGuid(), Guid.NewGuid(), EventCategory.BreakingNews, "Title", null, Severity.Critical, DateTimeOffset.UtcNow);

        Assert.False(rule.Matches(normalizedEvent));
    }

    [Fact]
    public void Matches_DifferentCategory_DoesNotMatch()
    {
        var rule = AlertRule.Create(
            Guid.NewGuid(), "Rule", EventCategory.BreakingNews, SeverityOnlyCriteria(Severity.Medium), DateTimeOffset.UtcNow);

        var normalizedEvent = NormalizedEvent.Create(
            Guid.NewGuid(), Guid.NewGuid(), EventCategory.MarketMovement, "Title", null, Severity.Critical, DateTimeOffset.UtcNow);

        Assert.False(rule.Matches(normalizedEvent));
    }

    [Fact]
    public void Matches_SameCategoryAndCriteriaSatisfied_Matches()
    {
        var rule = AlertRule.Create(
            Guid.NewGuid(), "Rule", EventCategory.NaturalDisaster, SeverityOnlyCriteria(Severity.High), DateTimeOffset.UtcNow);

        var normalizedEvent = NormalizedEvent.Create(
            Guid.NewGuid(), Guid.NewGuid(), EventCategory.NaturalDisaster, "Title", null, Severity.Critical, DateTimeOffset.UtcNow);

        Assert.True(rule.Matches(normalizedEvent));
    }

    [Fact]
    public void Enable_ReEnablesADisabledRule()
    {
        var rule = AlertRule.Create(
            Guid.NewGuid(), "Rule", EventCategory.BreakingNews, SeverityOnlyCriteria(Severity.Medium), DateTimeOffset.UtcNow);

        rule.Disable();
        rule.Enable();

        Assert.True(rule.IsEnabled);
    }

    [Fact]
    public void UpdateDetails_WithValidData_ReplacesNameCategoryAndCriteria()
    {
        var rule = AlertRule.Create(
            Guid.NewGuid(), "Original", EventCategory.BreakingNews, SeverityOnlyCriteria(Severity.Medium), DateTimeOffset.UtcNow);
        var newCriteria = SeverityOnlyCriteria(Severity.High);

        rule.UpdateDetails("Updated", EventCategory.NaturalDisaster, newCriteria);

        Assert.Equal("Updated", rule.Name);
        Assert.Equal(EventCategory.NaturalDisaster, rule.Category);
        Assert.Same(newCriteria, rule.Criteria);
    }

    [Fact]
    public void UpdateDetails_WithBlankName_Throws()
    {
        var rule = AlertRule.Create(
            Guid.NewGuid(), "Original", EventCategory.BreakingNews, SeverityOnlyCriteria(Severity.Medium), DateTimeOffset.UtcNow);

        var act = () => rule.UpdateDetails("   ", EventCategory.BreakingNews, SeverityOnlyCriteria(Severity.Medium));

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void UpdateDetails_WithUndefinedCategory_Throws()
    {
        var rule = AlertRule.Create(
            Guid.NewGuid(), "Original", EventCategory.BreakingNews, SeverityOnlyCriteria(Severity.Medium), DateTimeOffset.UtcNow);

        var act = () => rule.UpdateDetails("Updated", (EventCategory)999, SeverityOnlyCriteria(Severity.Medium));

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}
