using NotifyMe.Domain.Events;
using Xunit;

namespace NotifyMe.Domain.Tests.Events;

public class RawEventTests
{
    [Fact]
    public void Create_WithValidArguments_Succeeds()
    {
        var rawEvent = RawEvent.Create(
            Guid.NewGuid(), "usgs-earthquake-feed", EventCategory.NaturalDisaster, "{\"magnitude\":6.1}", DateTimeOffset.UtcNow);

        Assert.Equal("usgs-earthquake-feed", rawEvent.Source);
        Assert.Equal(EventCategory.NaturalDisaster, rawEvent.Category);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankSource_Throws(string? source)
    {
        var act = () => RawEvent.Create(Guid.NewGuid(), source!, EventCategory.BreakingNews, "payload", DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankPayload_Throws(string? payload)
    {
        var act = () => RawEvent.Create(Guid.NewGuid(), "source", EventCategory.BreakingNews, payload!, DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithUndefinedCategory_Throws()
    {
        var act = () => RawEvent.Create(Guid.NewGuid(), "source", (EventCategory)999, "payload", DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}
