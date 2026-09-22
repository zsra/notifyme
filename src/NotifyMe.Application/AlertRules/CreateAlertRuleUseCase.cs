using FluentValidation;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Alerts;

namespace NotifyMe.Application.AlertRules;

/// <summary>
/// Creates a new <see cref="AlertRule"/>. Input validation (required fields, defined enum
/// values) happens here via FluentValidation; the domain's own invariants (e.g. the
/// "no degenerate match criteria" rule in <see cref="MatchCriteria"/>) still apply on top and
/// surface as <see cref="ArgumentException"/> if violated.
/// </summary>
public sealed class CreateAlertRuleUseCase
{
    private readonly IAlertRuleRepository _alertRuleRepository;
    private readonly IValidator<CreateAlertRuleRequest> _validator;

    public CreateAlertRuleUseCase(IAlertRuleRepository alertRuleRepository, IValidator<CreateAlertRuleRequest> validator)
    {
        _alertRuleRepository = alertRuleRepository;
        _validator = validator;
    }

    public async Task<AlertRuleDto> ExecuteAsync(
        CreateAlertRuleRequest request, CancellationToken cancellationToken, Guid? ownerUserId = null)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var criteria = MatchCriteria.Create(request.Keywords, request.MinimumSeverity);
        var alertRule = AlertRule.Create(
            Guid.NewGuid(), request.Name, request.Category, criteria, DateTimeOffset.UtcNow, request.IsEnabled, ownerUserId);

        await _alertRuleRepository.AddAsync(alertRule, cancellationToken);

        return AlertRuleDto.FromEntity(alertRule);
    }
}
