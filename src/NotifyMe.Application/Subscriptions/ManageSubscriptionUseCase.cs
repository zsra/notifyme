using FluentValidation;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Common.Exceptions;
using NotifyMe.Domain.Subscriptions;

namespace NotifyMe.Application.Subscriptions;

/// <summary>
/// Links and unlinks <see cref="Domain.Alerts.AlertRule"/>s to <see cref="Domain.Channels.ChannelConfig"/>s.
/// Named "manage" (rather than separate create/delete use cases) per the Phase 04 plan, since
/// a <see cref="Subscription"/> has no independent lifecycle beyond "exists" or "doesn't".
/// </summary>
public sealed class ManageSubscriptionUseCase
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IAlertRuleRepository _alertRuleRepository;
    private readonly IChannelConfigRepository _channelConfigRepository;
    private readonly IValidator<CreateSubscriptionRequest> _validator;

    public ManageSubscriptionUseCase(
        ISubscriptionRepository subscriptionRepository,
        IAlertRuleRepository alertRuleRepository,
        IChannelConfigRepository channelConfigRepository,
        IValidator<CreateSubscriptionRequest> validator)
    {
        _subscriptionRepository = subscriptionRepository;
        _alertRuleRepository = alertRuleRepository;
        _channelConfigRepository = channelConfigRepository;
        _validator = validator;
    }

    public async Task<SubscriptionDto> SubscribeAsync(CreateSubscriptionRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var alertRule = await _alertRuleRepository.GetByIdAsync(request.AlertRuleId, cancellationToken)
            ?? throw new NotFoundException($"Alert rule '{request.AlertRuleId}' was not found.");

        var channelConfig = await _channelConfigRepository.GetByIdAsync(request.ChannelConfigId, cancellationToken)
            ?? throw new NotFoundException($"Channel '{request.ChannelConfigId}' was not found.");

        var subscription = Subscription.Create(Guid.NewGuid(), alertRule.Id, channelConfig.Id);
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        return SubscriptionDto.FromEntity(subscription);
    }

    public async Task UnsubscribeAsync(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken)
            ?? throw new NotFoundException($"Subscription '{subscriptionId}' was not found.");

        await _subscriptionRepository.DeleteAsync(subscription, cancellationToken);
    }
}
