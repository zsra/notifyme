using System.Security.Claims;
using NotifyMe.Api.Authentication;
using NotifyMe.Application.Subscriptions;

namespace NotifyMe.Api.Endpoints;

/// <summary>
/// Self-service subscription management, scoped to the caller. Mirrors
/// <see cref="MyAlertRulesEndpoints"/>; see ADR-0010. `SubscribeAsync` also verifies (in
/// <see cref="ManageSubscriptionUseCase"/>) that the referenced alert rule and channel both
/// belong to the same caller before linking them.
/// </summary>
public static class MySubscriptionsEndpoints
{
    public static void MapMySubscriptionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/subscriptions").WithTags("My Subscriptions");

        group.MapGet("/", async (
            Guid? alertRuleId, Guid? channelConfigId, ListSubscriptionsUseCase useCase,
            ClaimsPrincipal user, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(alertRuleId, channelConfigId, cancellationToken, user.GetUserId())))
            .Produces<IReadOnlyList<SubscriptionDto>>();

        group.MapPost("/", async (
            CreateSubscriptionRequest request, ManageSubscriptionUseCase useCase,
            ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            var dto = await useCase.SubscribeAsync(request, cancellationToken, user.GetUserId());
            return Results.Created($"/api/me/subscriptions/{dto.Id}", dto);
        })
            .Produces<SubscriptionDto>(StatusCodes.Status201Created);

        group.MapDelete("/{id:guid}", async (
            Guid id, ManageSubscriptionUseCase useCase, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            await useCase.UnsubscribeAsync(id, cancellationToken, user.GetUserId());
            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent);
    }
}
