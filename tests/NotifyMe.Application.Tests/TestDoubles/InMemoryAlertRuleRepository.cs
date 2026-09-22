using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Events;

namespace NotifyMe.Application.Tests.TestDoubles;

/// <summary>
/// Hand-rolled in-memory fake rather than a mocking library: keeps the Application layer's
/// test dependencies to just xUnit + FluentValidation (already needed for validators), and the
/// repository contracts are small enough that fakes are quick to write and easy to read.
/// </summary>
public sealed class InMemoryAlertRuleRepository : IAlertRuleRepository
{
    private readonly Dictionary<Guid, AlertRule> _alertRules = new();

    public IReadOnlyCollection<AlertRule> Seeded => _alertRules.Values;

    public void Seed(params AlertRule[] alertRules)
    {
        foreach (var alertRule in alertRules)
        {
            _alertRules[alertRule.Id] = alertRule;
        }
    }

    public Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_alertRules.GetValueOrDefault(id));

    public Task<IReadOnlyList<AlertRule>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AlertRule>>(_alertRules.Values.ToList());

    public Task<IReadOnlyList<AlertRule>> ListEnabledByCategoryAsync(EventCategory category, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AlertRule>>(
            _alertRules.Values.Where(rule => rule.IsEnabled && rule.Category == category).ToList());

    public Task AddAsync(AlertRule alertRule, CancellationToken cancellationToken)
    {
        _alertRules[alertRule.Id] = alertRule;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AlertRule alertRule, CancellationToken cancellationToken)
    {
        _alertRules[alertRule.Id] = alertRule;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AlertRule alertRule, CancellationToken cancellationToken)
    {
        _alertRules.Remove(alertRule.Id);
        return Task.CompletedTask;
    }
}
