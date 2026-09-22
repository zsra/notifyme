using NotifyMe.Domain.Channels;
using Xunit;

namespace NotifyMe.Domain.Tests.Channels;

public class ChannelConfigTests
{
    [Fact]
    public void Create_NormalizesChannelTypeToLowercase()
    {
        var channel = ChannelConfig.Create(Guid.NewGuid(), "  Slack  ", "https://hooks.slack.com/services/PLACEHOLDER");

        Assert.Equal("slack", channel.ChannelType);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankChannelType_Throws(string? channelType)
    {
        var act = () => ChannelConfig.Create(Guid.NewGuid(), channelType!, "target@example.com");

        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankTarget_Throws(string? target)
    {
        var act = () => ChannelConfig.Create(Guid.NewGuid(), "email", target!);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_DefaultsToEnabled()
    {
        var channel = ChannelConfig.Create(Guid.NewGuid(), "email", "target@example.com");

        Assert.True(channel.IsEnabled);
    }

    [Fact]
    public void Disable_ThenEnable_TogglesState()
    {
        var channel = ChannelConfig.Create(Guid.NewGuid(), "email", "target@example.com");

        channel.Disable();
        Assert.False(channel.IsEnabled);

        channel.Enable();
        Assert.True(channel.IsEnabled);
    }
}
