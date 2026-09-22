using FluentValidation;

namespace NotifyMe.Application.Subscriptions;

public sealed class CreateSubscriptionRequestValidator : AbstractValidator<CreateSubscriptionRequest>
{
    public CreateSubscriptionRequestValidator()
    {
        RuleFor(request => request.AlertRuleId).NotEmpty();
        RuleFor(request => request.ChannelConfigId).NotEmpty();
    }
}
