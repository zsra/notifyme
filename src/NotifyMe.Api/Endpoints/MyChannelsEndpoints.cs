using System.Security.Claims;
using NotifyMe.Api.Authentication;
using NotifyMe.Application.Channels;

namespace NotifyMe.Api.Endpoints;

/// <summary>
/// Self-service channel management, scoped to the caller. Mirrors <see cref="MyAlertRulesEndpoints"/>;
/// see ADR-0010.
/// </summary>
public static class MyChannelsEndpoints
{
    public static void MapMyChannelsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/channels").WithTags("My Channels");

        group.MapGet("/", async (
            string? channelType, bool? isEnabled, ListChannelConfigsUseCase useCase,
            ClaimsPrincipal user, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(channelType, isEnabled, cancellationToken, user.GetUserId())))
            .Produces<IReadOnlyList<ChannelConfigDto>>();

        group.MapGet("/{id:guid}", async (
            Guid id, GetChannelConfigUseCase useCase, ClaimsPrincipal user, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(id, cancellationToken, user.GetUserId())))
            .Produces<ChannelConfigDto>();

        group.MapPost("/", async (
            CreateChannelConfigRequest request, CreateChannelConfigUseCase useCase,
            ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            var dto = await useCase.ExecuteAsync(request, cancellationToken, user.GetUserId());
            return Results.Created($"/api/me/channels/{dto.Id}", dto);
        })
            .Produces<ChannelConfigDto>(StatusCodes.Status201Created);

        group.MapPut("/{id:guid}", async (
            Guid id, UpdateChannelConfigRequest request, UpdateChannelConfigUseCase useCase,
            ClaimsPrincipal user, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(request with { Id = id }, cancellationToken, user.GetUserId())))
            .Produces<ChannelConfigDto>();

        group.MapDelete("/{id:guid}", async (
            Guid id, DeleteChannelConfigUseCase useCase, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            await useCase.ExecuteAsync(id, cancellationToken, user.GetUserId());
            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent);
    }
}
