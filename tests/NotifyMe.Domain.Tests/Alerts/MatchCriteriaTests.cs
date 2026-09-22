using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Common;
using Xunit;

namespace NotifyMe.Domain.Tests.Alerts;

public class MatchCriteriaTests
{
    [Fact]
    public void Create_WithNoKeywordsAndLowSeverity_Throws()
    {
        var act = () => MatchCriteria.Create(keywords: null, Severity.Low);

        var ex = Assert.Throws<ArgumentException>(act);
        Assert.Contains("at least one real match criterion", ex.Message);
    }

    [Fact]
    public void Create_WithOnlyKeywords_Succeeds()
    {
        var criteria = MatchCriteria.Create(new[] { "earthquake" }, Severity.Low);

        Assert.Single(criteria.Keywords);
        Assert.Equal(Severity.Low, criteria.MinimumSeverity);
    }

    [Fact]
    public void Create_WithOnlySeverityAboveLow_Succeeds()
    {
        var criteria = MatchCriteria.Create(keywords: null, Severity.High);

        Assert.Empty(criteria.Keywords);
    }

    [Fact]
    public void Create_DeduplicatesKeywordsCaseInsensitively()
    {
        var criteria = MatchCriteria.Create(new[] { "Earthquake", "earthquake", " EARTHQUAKE " }, Severity.Low);

        Assert.Single(criteria.Keywords);
    }

    [Fact]
    public void IsSatisfiedBy_EventBelowMinimumSeverity_ReturnsFalse()
    {
        var criteria = MatchCriteria.Create(keywords: null, Severity.High);

        var result = criteria.IsSatisfiedBy("Some title", "Some description", Severity.Medium);

        Assert.False(result);
    }

    [Fact]
    public void IsSatisfiedBy_NoKeywordsAndSeverityMet_ReturnsTrue()
    {
        var criteria = MatchCriteria.Create(keywords: null, Severity.Medium);

        var result = criteria.IsSatisfiedBy("Anything", "Anything", Severity.Critical);

        Assert.True(result);
    }

    [Theory]
    [InlineData("Magnitude 7 earthquake strikes coast", "", true)]
    [InlineData("Quiet day in the markets", "No earthquake reported", true)]
    [InlineData("Nothing relevant happened", "Still nothing relevant", false)]
    public void IsSatisfiedBy_MatchesKeywordInTitleOrDescription(string title, string description, bool expected)
    {
        var criteria = MatchCriteria.Create(new[] { "earthquake" }, Severity.Low);

        var result = criteria.IsSatisfiedBy(title, description, Severity.Low);

        Assert.Equal(expected, result);
    }
}
