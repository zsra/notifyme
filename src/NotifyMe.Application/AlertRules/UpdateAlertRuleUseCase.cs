using FluentValidation;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Common.Exceptions;
using NotifyMe.Domain.Alerts;

namespace NotifyMe.Application.AlertRules;

public sealed class UpdateAlertRuleUseCase
{
    private readonly IAlertRuleRepository _alertRuleRepository;
    private readonly IValidator<UpdateAlertRuleRequest> _validator;

    public UpdateAlertRuleUseCase(IAlertRuleRepository alertRuleRepository, IValidator<UpdateAlertRuleRequest> validator)
    {
        _alertRuleRepository = alertRuleRepository;
        _validator = validator;
    }

    public async Task<AlertRuleDto> ExecuteAsync(
        UpdateAlertRuleRequest request, CancellationToken cancellationToken, Guid? ownerUserId = null)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var alertRule = await _alertRuleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Alert rule '{request.Id}' was not found.");

        if (ownerUserId is not null && alertRule.OwnerUserId != ownerUserId)
        {
            throw new NotFoundException($"Alert rule '{request.Id}' was not found.");
        }

        var criteria = MatchCriteria.Create(request.Keywords, request.MinimumSeverity);
        alertRule.UpdateDetails(request.Name, request.Category, criteria);

        await _alertRuleRepository.UpdateAsync(alertRule, cancellationToken);

        return AlertRuleDto.FromEntity(alertRule);
    }
}
