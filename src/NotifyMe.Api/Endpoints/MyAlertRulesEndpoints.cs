using System.Security.Claims;
using NotifyMe.Api.Authentication;
using NotifyMe.Application.AlertRules;
using NotifyMe.Domain.Events;

namespace NotifyMe.Api.Endpoints;

/// <summary>
/// Self-service alert rule management, scoped to the caller (see ADR-0010): every use case call
/// passes the JWT's user id as `ownerUserId`, so a user only ever sees/edits their own rules,
/// reusing the exact same use cases the Admin API calls with `ownerUserId: null`.
/// </summary>
public static class MyAlertRulesEndpoints
{
    public static void MapMyAlertRulesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/alert-rules").WithTags("My Alert Rules");

        group.MapGet("/", async (
            EventCategory? eventCategory, bool? isEnabled, ListAlertRulesUseCase useCase,
            ClaimsPrincipal user, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(eventCategory, isEnabled, cancellationToken, user.GetUserId())))
            .Produces<IReadOnlyList<AlertRuleDto>>();

        group.MapGet("/{id:guid}", async (
            Guid id, GetAlertRuleUseCase useCase, ClaimsPrincipal user, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(id, cancellationToken, user.GetUserId())))
            .Produces<AlertRuleDto>();

        group.MapPost("/", async (
            CreateAlertRuleRequest request, CreateAlertRuleUseCase useCase,
            ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            var dto = await useCase.ExecuteAsync(request, cancellationToken, user.GetUserId());
            return Results.Created($"/api/me/alert-rules/{dto.Id}", dto);
        })
            .Produces<AlertRuleDto>(StatusCodes.Status201Created);

        group.MapPut("/{id:guid}", async (
            Guid id, UpdateAlertRuleRequest request, UpdateAlertRuleUseCase useCase,
            ClaimsPrincipal user, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(request with { Id = id }, cancellationToken, user.GetUserId())))
            .Produces<AlertRuleDto>();

        group.MapDelete("/{id:guid}", async (
            Guid id, DeleteAlertRuleUseCase useCase, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            await useCase.ExecuteAsync(id, cancellationToken, user.GetUserId());
            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent);
    }
}

