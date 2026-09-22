using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using Xunit;

namespace NotifyMe.Domain.Tests.Events;

public class NormalizedEventTests
{
    [Fact]
    public void Create_WithValidArguments_TrimsTitleAndDescription()
    {
        var normalized = NormalizedEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EventCategory.MarketMovement,
            "  S&P 500 drops 5%  ",
            "  Broad sell-off across sectors  ",
            Severity.High,
            DateTimeOffset.UtcNow);

        Assert.Equal("S&P 500 drops 5%", normalized.Title);
        Assert.Equal("Broad sell-off across sectors", normalized.Description);
    }

    [Fact]
    public void Create_WithNullDescription_DefaultsToEmpty()
    {
        var normalized = NormalizedEvent.Create(
            Guid.NewGuid(), Guid.NewGuid(), EventCategory.BreakingNews, "Title", null, Severity.Low, DateTimeOffset.UtcNow);

        Assert.Equal(string.Empty, normalized.Description);
    }

    [Fact]
    public void Create_WithEmptyRawEventId_Throws()
    {
        var act = () => NormalizedEvent.Create(
            Guid.NewGuid(), Guid.Empty, EventCategory.BreakingNews, "Title", null, Severity.Low, DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankTitle_Throws(string? title)
    {
        var act = () => NormalizedEvent.Create(
            Guid.NewGuid(), Guid.NewGuid(), EventCategory.BreakingNews, title!, null, Severity.Low, DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(act);
    }
}
