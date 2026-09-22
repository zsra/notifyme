using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Common.Exceptions;

namespace NotifyMe.Application.AlertRules;

public sealed class GetAlertRuleUseCase
{
    private readonly IAlertRuleRepository _alertRuleRepository;

    public GetAlertRuleUseCase(IAlertRuleRepository alertRuleRepository)
    {
        _alertRuleRepository = alertRuleRepository;
    }

    public async Task<AlertRuleDto> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var alertRule = await _alertRuleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Alert rule '{id}' was not found.");

        return AlertRuleDto.FromEntity(alertRule);
    }
}
