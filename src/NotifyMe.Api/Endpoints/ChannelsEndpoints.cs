using NotifyMe.Application.Channels;

namespace NotifyMe.Api.Endpoints;

public static class ChannelsEndpoints
{
    public static void MapChannelsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/channels").WithTags("Channels");

        group.MapGet("/", async (
            string? channelType, bool? isEnabled, ListChannelConfigsUseCase useCase, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(channelType, isEnabled, cancellationToken)))
            .Produces<IReadOnlyList<ChannelConfigDto>>();

        group.MapGet("/{id:guid}", async (Guid id, GetChannelConfigUseCase useCase, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(id, cancellationToken)))
            .Produces<ChannelConfigDto>();

        group.MapPost("/", async (
            CreateChannelConfigRequest request, CreateChannelConfigUseCase useCase, CancellationToken cancellationToken) =>
        {
            var dto = await useCase.ExecuteAsync(request, cancellationToken);
            return Results.Created($"/api/admin/channels/{dto.Id}", dto);
        })
            .Produces<ChannelConfigDto>(StatusCodes.Status201Created);

        group.MapPut("/{id:guid}", async (
            Guid id, UpdateChannelConfigRequest request, UpdateChannelConfigUseCase useCase, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(request with { Id = id }, cancellationToken)))
            .Produces<ChannelConfigDto>();

        group.MapDelete("/{id:guid}", async (Guid id, DeleteChannelConfigUseCase useCase, CancellationToken cancellationToken) =>
        {
            await useCase.ExecuteAsync(id, cancellationToken);
            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent);
    }
}
