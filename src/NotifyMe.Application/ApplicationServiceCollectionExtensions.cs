using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotifyMe.Application.AlertRules;
using NotifyMe.Application.Alerts;
using NotifyMe.Application.Channels;
using NotifyMe.Application.Common.Resilience;
using NotifyMe.Application.Events;
using NotifyMe.Application.Notifications;
using NotifyMe.Application.Subscriptions;
using NotifyMe.Domain.Abstractions;
using Polly;

namespace NotifyMe.Application;

/// <summary>
/// Registers every Application-layer use case, service, and FluentValidation validator, so the
/// Api composition root (Phase 08) just calls one method rather than listing every use case
/// itself. Mirrors the per-layer `AddNotifyMeXxx` extension pattern already used by
/// `Infrastructure` (see `PersistenceServiceCollectionExtensions`,
/// `EventSourcesServiceCollectionExtensions`, `NotificationChannelsServiceCollectionExtensions`).
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddNotifyMeApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ChannelSendResilienceOptions>(configuration.GetSection(ChannelSendResilienceOptions.SectionName));
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ChannelSendResilienceOptions>>().Value;
            var builder = new ResiliencePipelineBuilder<NotificationDispatchResult>();

            if (options.MaxRetryAttempts > 0)
            {
                builder.AddRetry(new Polly.Retry.RetryStrategyOptions<NotificationDispatchResult>
                {
                    ShouldHandle = new PredicateBuilder<NotificationDispatchResult>().HandleResult(result => !result.IsSuccess),
                    MaxRetryAttempts = options.MaxRetryAttempts,
                    Delay = options.BaseDelay,
                    BackoffType = DelayBackoffType.Exponential,
                });
            }

            return builder.Build();
        });

        services.AddScoped<IAlertMatcher, AlertMatcher>();
        services.AddScoped<EvaluateAlertRulesService>();
        services.AddScoped<DispatchNotificationUseCase>();
        services.AddScoped<IngestEventsUseCase>();

        services.AddScoped<IValidator<CreateAlertRuleRequest>, CreateAlertRuleRequestValidator>();
        services.AddScoped<CreateAlertRuleUseCase>();
        services.AddScoped<IValidator<UpdateAlertRuleRequest>, UpdateAlertRuleRequestValidator>();
        services.AddScoped<UpdateAlertRuleUseCase>();
        services.AddScoped<GetAlertRuleUseCase>();
        services.AddScoped<ListAlertRulesUseCase>();
        services.AddScoped<DeleteAlertRuleUseCase>();

        services.AddScoped<IValidator<CreateChannelConfigRequest>, CreateChannelConfigRequestValidator>();
        services.AddScoped<CreateChannelConfigUseCase>();
        services.AddScoped<IValidator<UpdateChannelConfigRequest>, UpdateChannelConfigRequestValidator>();
        services.AddScoped<UpdateChannelConfigUseCase>();
        services.AddScoped<GetChannelConfigUseCase>();
        services.AddScoped<ListChannelConfigsUseCase>();
        services.AddScoped<DeleteChannelConfigUseCase>();

        services.AddScoped<IValidator<CreateSubscriptionRequest>, CreateSubscriptionRequestValidator>();
        services.AddScoped<ManageSubscriptionUseCase>();
        services.AddScoped<ListSubscriptionsUseCase>();

        services.AddScoped<GetNotificationUseCase>();
        services.AddScoped<ListNotificationsUseCase>();

        return services;
    }
}
