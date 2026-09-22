using NotifyMe.Application.AlertRules;
using NotifyMe.Application.Common.Exceptions;
using NotifyMe.Application.Tests.TestDoubles;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using Xunit;

namespace NotifyMe.Application.Tests.AlertRules;

public class UpdateAlertRuleUseCaseTests
{
    private static UpdateAlertRuleUseCase CreateUseCase(out InMemoryAlertRuleRepository repository)
    {
        repository = new InMemoryAlertRuleRepository();
        return new UpdateAlertRuleUseCase(repository, new UpdateAlertRuleRequestValidator());
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingRule_UpdatesAndReturnsDto()
    {
        var useCase = CreateUseCase(out var repository);
        var criteria = MatchCriteria.Create(new[] { "flood" }, Severity.Low);
        var rule = AlertRule.Create(Guid.NewGuid(), "Original", EventCategory.NaturalDisaster, criteria, DateTimeOffset.UtcNow);
        repository.Seed(rule);

        var request = new UpdateAlertRuleRequest(
            rule.Id, "Renamed", EventCategory.MarketMovement, new[] { "crash" }, Severity.High);

        var dto = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal("Renamed", dto.Name);
        Assert.Equal(EventCategory.MarketMovement, dto.Category);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownId_ThrowsNotFoundException()
    {
        var useCase = CreateUseCase(out _);
        var request = new UpdateAlertRuleRequest(
            Guid.NewGuid(), "Renamed", EventCategory.MarketMovement, new[] { "crash" }, Severity.High);

        await Assert.ThrowsAsync<NotFoundException>(() => useCase.ExecuteAsync(request, CancellationToken.None));
    }
}
