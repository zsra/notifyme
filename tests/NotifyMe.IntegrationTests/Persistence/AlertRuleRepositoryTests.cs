using Microsoft.EntityFrameworkCore;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using NotifyMe.Infrastructure.Persistence;
using NotifyMe.Infrastructure.Persistence.Repositories;
using Xunit;

namespace NotifyMe.IntegrationTests.Persistence;

/// <summary>
/// Round-trips an <see cref="AlertRule"/> (including its owned <see cref="MatchCriteria"/> value
/// object) through <see cref="AlertRuleRepository"/> against a real PostgreSQL instance.
///
/// Requires the local docker-compose Postgres service to be running first:
/// `docker compose up -d postgres`. This is the "small integration test against the
/// docker-compose Postgres instance" called for by Phase 05; a Testcontainers-based, fully
/// self-contained equivalent is planned for Phase 11 alongside the rest of the API integration
/// suite.
/// </summary>
public class AlertRuleRepositoryTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=notifyme;Username=notifyme;Password=notifyme_dev_only";

    private NotifyMeDbContext _dbContext = null!;
    private AlertRuleRepository _repository = null!;

    public Task InitializeAsync()
    {
        _dbContext = CreateDbContext();
        _repository = new AlertRuleRepository(_dbContext);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    private static NotifyMeDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<NotifyMeDbContext>().UseNpgsql(ConnectionString).Options);

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_RoundTripsAlertRuleAndMatchCriteria()
    {
        var criteria = MatchCriteria.Create(new[] { "earthquake", "flood" }, Severity.Medium);
        var alertRule = AlertRule.Create(
            Guid.NewGuid(), "Natural disaster watch", EventCategory.NaturalDisaster, criteria, DateTimeOffset.UtcNow);

        await _repository.AddAsync(alertRule, CancellationToken.None);

        // Read back through a separate DbContext/repository so this actually hits the database
        // rather than returning the same tracked in-memory instance.
        await using var readDbContext = CreateDbContext();
        var readRepository = new AlertRuleRepository(readDbContext);
        var loaded = await readRepository.GetByIdAsync(alertRule.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(alertRule.Name, loaded!.Name);
        Assert.Equal(alertRule.Category, loaded.Category);
        Assert.Equal(alertRule.IsEnabled, loaded.IsEnabled);
        Assert.Equal(2, loaded.Criteria.Keywords.Count);
        Assert.Contains("earthquake", loaded.Criteria.Keywords);
        Assert.Contains("flood", loaded.Criteria.Keywords);
        Assert.Equal(Severity.Medium, loaded.Criteria.MinimumSeverity);

        await readRepository.DeleteAsync(loaded, CancellationToken.None);
    }

    [Fact]
    public async Task ListEnabledByCategoryAsync_ExcludesDisabledAndOtherCategoryRules()
    {
        var matchingRule = AlertRule.Create(
            Guid.NewGuid(), "Matching rule", EventCategory.MarketMovement,
            MatchCriteria.Create(keywords: null, Severity.Medium), DateTimeOffset.UtcNow);
        var disabledRule = AlertRule.Create(
            Guid.NewGuid(), "Disabled rule", EventCategory.MarketMovement,
            MatchCriteria.Create(keywords: null, Severity.Medium), DateTimeOffset.UtcNow, isEnabled: false);
        var otherCategoryRule = AlertRule.Create(
            Guid.NewGuid(), "Other category rule", EventCategory.BreakingNews,
            MatchCriteria.Create(keywords: null, Severity.Medium), DateTimeOffset.UtcNow);

        await _repository.AddAsync(matchingRule, CancellationToken.None);
        await _repository.AddAsync(disabledRule, CancellationToken.None);
        await _repository.AddAsync(otherCategoryRule, CancellationToken.None);

        await using var readDbContext = CreateDbContext();
        var readRepository = new AlertRuleRepository(readDbContext);
        var results = await readRepository.ListEnabledByCategoryAsync(EventCategory.MarketMovement, CancellationToken.None);

        Assert.Contains(results, rule => rule.Id == matchingRule.Id);
        Assert.DoesNotContain(results, rule => rule.Id == disabledRule.Id);
        Assert.DoesNotContain(results, rule => rule.Id == otherCategoryRule.Id);

        await readRepository.DeleteAsync(matchingRule, CancellationToken.None);
        await readRepository.DeleteAsync(disabledRule, CancellationToken.None);
        await readRepository.DeleteAsync(otherCategoryRule, CancellationToken.None);
    }
}
