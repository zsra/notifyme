using NotifyMe.Application.AlertRules;
using NotifyMe.Domain.Events;

namespace NotifyMe.Api.Endpoints;

public static class AlertRulesEndpoints
{
    public static void MapAlertRulesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/alert-rules").WithTags("Alert Rules");

        group.MapGet("/", async (
            EventCategory? eventCategory, bool? isEnabled, ListAlertRulesUseCase useCase, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(eventCategory, isEnabled, cancellationToken)))
            .Produces<IReadOnlyList<AlertRuleDto>>();

        group.MapGet("/{id:guid}", async (Guid id, GetAlertRuleUseCase useCase, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(id, cancellationToken)))
            .Produces<AlertRuleDto>();

        group.MapPost("/", async (
            CreateAlertRuleRequest request, CreateAlertRuleUseCase useCase, CancellationToken cancellationToken) =>
        {
            var dto = await useCase.ExecuteAsync(request, cancellationToken);
            return Results.Created($"/api/admin/alert-rules/{dto.Id}", dto);
        })
            .Produces<AlertRuleDto>(StatusCodes.Status201Created);

        group.MapPut("/{id:guid}", async (
            Guid id, UpdateAlertRuleRequest request, UpdateAlertRuleUseCase useCase, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(request with { Id = id }, cancellationToken)))
            .Produces<AlertRuleDto>();

        group.MapDelete("/{id:guid}", async (Guid id, DeleteAlertRuleUseCase useCase, CancellationToken cancellationToken) =>
        {
            await useCase.ExecuteAsync(id, cancellationToken);
            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent);
    }
}
