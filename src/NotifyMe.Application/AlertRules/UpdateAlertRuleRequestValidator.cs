using FluentValidation;

namespace NotifyMe.Application.AlertRules;

public sealed class UpdateAlertRuleRequestValidator : AbstractValidator<UpdateAlertRuleRequest>
{
    public UpdateAlertRuleRequestValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.Category).IsInEnum();
        RuleFor(request => request.MinimumSeverity).IsInEnum();
    }
}
