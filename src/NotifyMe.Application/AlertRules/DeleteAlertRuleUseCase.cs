using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Common.Exceptions;

namespace NotifyMe.Application.AlertRules;

public sealed class DeleteAlertRuleUseCase
{
    private readonly IAlertRuleRepository _alertRuleRepository;

    public DeleteAlertRuleUseCase(IAlertRuleRepository alertRuleRepository)
    {
        _alertRuleRepository = alertRuleRepository;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var alertRule = await _alertRuleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Alert rule '{id}' was not found.");

        await _alertRuleRepository.DeleteAsync(alertRule, cancellationToken);
    }
}
