using NotifyMe.Domain.Notifications;
using Xunit;

namespace NotifyMe.Domain.Tests.Notifications;

public class NotificationTests
{
    private static Notification CreatePending() =>
        Notification.CreatePending(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void CreatePending_StartsInPendingStatus()
    {
        var notification = CreatePending();

        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.Null(notification.SentAt);
        Assert.Null(notification.Error);
    }

    [Fact]
    public void CreatePending_WithEmptyAlertRuleId_Throws()
    {
        var act = () => Notification.CreatePending(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void MarkSent_FromPending_SetsStatusAndTimestamp()
    {
        var notification = CreatePending();
        var sentAt = DateTimeOffset.UtcNow;

        notification.MarkSent(sentAt);

        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.Equal(sentAt, notification.SentAt);
    }

    [Fact]
    public void MarkFailed_FromPending_SetsStatusAndError()
    {
        var notification = CreatePending();

        notification.MarkFailed("SMTP timeout");

        Assert.Equal(NotificationStatus.Failed, notification.Status);
        Assert.Equal("SMTP timeout", notification.Error);
    }

    [Fact]
    public void MarkFailed_WithBlankError_Throws()
    {
        var notification = CreatePending();

        var act = () => notification.MarkFailed("   ");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void MarkSent_AlreadySent_Throws()
    {
        var notification = CreatePending();
        notification.MarkSent(DateTimeOffset.UtcNow);

        var act = () => notification.MarkSent(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void MarkFailed_AlreadyFailed_Throws()
    {
        var notification = CreatePending();
        notification.MarkFailed("first failure");

        var act = () => notification.MarkFailed("second failure");

        Assert.Throws<InvalidOperationException>(act);
    }
}
