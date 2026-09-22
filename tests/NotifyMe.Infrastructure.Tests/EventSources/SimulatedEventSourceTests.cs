using Microsoft.Extensions.Options;
using NotifyMe.Domain.Events;
using NotifyMe.Infrastructure.EventSources.Simulated;
using Xunit;

namespace NotifyMe.Infrastructure.Tests.EventSources;

public class SimulatedEventSourceTests
{
    private static SimulatedEventSource CreateSource(SimulatedEventSourceOptions options) =>
        new(Options.Create(options));

    [Fact]
    public async Task FetchAsync_WithFixedSeed_ProducesDeterministicSequenceAcrossInstances()
    {
        var options = new SimulatedEventSourceOptions { Seed = 42 };
        var first = CreateSource(options);
        var second = CreateSource(options);

        for (var i = 0; i < 20; i++)
        {
            var firstBatch = await first.FetchAsync(CancellationToken.None);
            var secondBatch = await second.FetchAsync(CancellationToken.None);

            Assert.Equal(firstBatch.Count, secondBatch.Count);
            for (var j = 0; j < firstBatch.Count; j++)
            {
                Assert.Equal(firstBatch[j].Category, secondBatch[j].Category);
                Assert.Equal(firstBatch[j].Source, secondBatch[j].Source);
                Assert.Equal(firstBatch[j].Payload, secondBatch[j].Payload);
            }
        }
    }

    [Fact]
    public async Task FetchAsync_OverManyIterations_ProducesAllThreeEventCategories()
    {
        var source = CreateSource(new SimulatedEventSourceOptions { Seed = 7, MinEventsPerFetch = 2, MaxEventsPerFetch = 4 });
        var seenCategories = new HashSet<EventCategory>();

        for (var i = 0; i < 100; i++)
        {
            var batch = await source.FetchAsync(CancellationToken.None);
            foreach (var rawEvent in batch)
            {
                seenCategories.Add(rawEvent.Category);
            }
        }

        Assert.Equal(3, seenCategories.Count);
        Assert.Contains(EventCategory.BreakingNews, seenCategories);
        Assert.Contains(EventCategory.MarketMovement, seenCategories);
        Assert.Contains(EventCategory.NaturalDisaster, seenCategories);
    }

    [Fact]
    public async Task FetchAsync_RespectsConfiguredMinAndMaxEventsPerFetch()
    {
        var source = CreateSource(new SimulatedEventSourceOptions { Seed = 1, MinEventsPerFetch = 2, MaxEventsPerFetch = 2 });

        for (var i = 0; i < 10; i++)
        {
            var batch = await source.FetchAsync(CancellationToken.None);
            Assert.Equal(2, batch.Count);
        }
    }
}
