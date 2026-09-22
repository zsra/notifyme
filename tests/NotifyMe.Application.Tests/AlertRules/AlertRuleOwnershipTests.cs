using NotifyMe.Application.AlertRules;
using NotifyMe.Application.Common.Exceptions;
using NotifyMe.Application.Tests.TestDoubles;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using Xunit;

namespace NotifyMe.Application.Tests.AlertRules;

/// <summary>
/// Covers the Phase 16 self-service ownership scoping added to the existing AlertRule use
/// cases (see ADR-0010): an `ownerUserId` of `null` (the Admin API's call pattern) is
/// unrestricted, exactly as before; a non-null `ownerUserId` (the `/api/me` call pattern) is
/// restricted to that user's own rows.
/// </summary>
public class AlertRuleOwnershipTests
{
    private static AlertRule CreateRule(Guid? ownerUserId) =>
        AlertRule.Create(
            Guid.NewGuid(), "Rule", EventCategory.NaturalDisaster,
            MatchCriteria.Create(new[] { "flood" }, Severity.Low), DateTimeOffset.UtcNow, ownerUserId: ownerUserId);

    [Fact]
    public async Task ListAlertRules_WithOwnerUserId_OnlyReturnsThatUsersRules()
    {
        var repository = new InMemoryAlertRuleRepository();
        var ownerA = Guid.NewGuid();
        var ownerB = Guid.NewGuid();
        repository.Seed(CreateRule(ownerA), CreateRule(ownerB), CreateRule(null));
        var useCase = new ListAlertRulesUseCase(repository);

        var result = await useCase.ExecuteAsync(null, null, CancellationToken.None, ownerA);

        var dto = Assert.Single(result);
        Assert.Equal(ownerA, repository.Seeded.Single(rule => rule.Id == dto.Id).OwnerUserId);
    }

    [Fact]
    public async Task ListAlertRules_WithoutOwnerUserId_ReturnsEverything()
    {
        var repository = new InMemoryAlertRuleRepository();
        repository.Seed(CreateRule(Guid.NewGuid()), CreateRule(Guid.NewGuid()), CreateRule(null));
        var useCase = new ListAlertRulesUseCase(repository);

        var result = await useCase.ExecuteAsync(null, null, CancellationToken.None);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAlertRule_OwnedByAnotherUser_ThrowsNotFoundException()
    {
        var repository = new InMemoryAlertRuleRepository();
        var rule = CreateRule(Guid.NewGuid());
        repository.Seed(rule);
        var useCase = new GetAlertRuleUseCase(repository);

        await Assert.ThrowsAsync<NotFoundException>(
            () => useCase.ExecuteAsync(rule.Id, CancellationToken.None, Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAlertRule_OwnedByCaller_Succeeds()
    {
        var repository = new InMemoryAlertRuleRepository();
        var ownerId = Guid.NewGuid();
        var rule = CreateRule(ownerId);
        repository.Seed(rule);
        var useCase = new DeleteAlertRuleUseCase(repository);

        await useCase.ExecuteAsync(rule.Id, CancellationToken.None, ownerId);

        Assert.Empty(repository.Seeded);
    }
}
