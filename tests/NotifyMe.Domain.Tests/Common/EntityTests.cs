using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using Xunit;

namespace NotifyMe.Domain.Tests.Common;

public class EntityTests
{
    private sealed class TestEntity : Entity
    {
        public TestEntity(Guid id)
            : base(id)
        {
        }
    }

    [Fact]
    public void Constructor_WithEmptyGuid_Throws()
    {
        var act = () => new TestEntity(Guid.Empty);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Equals_SameTypeAndId_AreEqual()
    {
        var id = Guid.NewGuid();
        var first = new TestEntity(id);
        var second = new TestEntity(id);

        Assert.Equal(first, second);
        Assert.True(first == second);
    }

    [Fact]
    public void Equals_DifferentTypesSameId_AreNotEqual()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        var rawEvent = RawEvent.Create(id, "source", EventCategory.BreakingNews, "payload", DateTimeOffset.UtcNow);

        Assert.False(entity.Equals(rawEvent));
    }
}
