using NotifyMe.Application.Subscriptions;

namespace NotifyMe.Api.Endpoints;

public static class SubscriptionsEndpoints
{
    public static void MapSubscriptionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/subscriptions").WithTags("Subscriptions");

        group.MapGet("/", async (
            Guid? alertRuleId, Guid? channelConfigId, ListSubscriptionsUseCase useCase, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(alertRuleId, channelConfigId, cancellationToken)));

        group.MapPost("/", async (
            CreateSubscriptionRequest request, ManageSubscriptionUseCase useCase, CancellationToken cancellationToken) =>
        {
            var dto = await useCase.SubscribeAsync(request, cancellationToken);
            return Results.Created($"/api/admin/subscriptions/{dto.Id}", dto);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ManageSubscriptionUseCase useCase, CancellationToken cancellationToken) =>
        {
            await useCase.UnsubscribeAsync(id, cancellationToken);
            return Results.NoContent();
        });
    }
}
