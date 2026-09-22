using NotifyMe.Application.Notifications;
using NotifyMe.Domain.Notifications;

namespace NotifyMe.Api.Endpoints;

public static class NotificationsEndpoints
{
    public static void MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/notifications").WithTags("Notifications");

        group.MapGet("/", async (
            NotificationStatus? status,
            Guid? alertRuleId,
            DateTimeOffset? sentFrom,
            DateTimeOffset? sentTo,
            ListNotificationsUseCase useCase,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(status, alertRuleId, sentFrom, sentTo, cancellationToken)))
            .Produces<IReadOnlyList<NotificationDto>>();

        group.MapGet("/{id:guid}", async (Guid id, GetNotificationUseCase useCase, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(id, cancellationToken)))
            .Produces<NotificationDto>();
    }
}
