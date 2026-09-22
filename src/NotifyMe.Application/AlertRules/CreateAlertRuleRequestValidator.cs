using FluentValidation;

namespace NotifyMe.Application.AlertRules;

public sealed class CreateAlertRuleRequestValidator : AbstractValidator<CreateAlertRuleRequest>
{
    public CreateAlertRuleRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.Category).IsInEnum();
        RuleFor(request => request.MinimumSeverity).IsInEnum();
    }
}
