using NotifyMe.Application.Events;

namespace NotifyMe.Api.Endpoints;

/// <summary>
/// The manual demo/testing helper from docs/api/admin-api.md. Forces one ingestion pass right
/// now instead of waiting on <c>SimulatedEventSource</c>'s polling interval. Forcing a specific
/// <c>eventCategory</c> (as originally sketched in the draft doc) isn't supported: the
/// <c>IEventSource</c> abstraction has no notion of "fetch only this category", and adding one
/// just for this demo endpoint would leak a simulator-specific concern into the Domain
/// abstraction - see docs/architecture/event-ingestion.md.
/// </summary>
public static class EventsEndpoints
{
    public static void MapEventsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/events").WithTags("Events");

        group.MapPost("/trigger-simulated", async (IngestEventsUseCase useCase, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(cancellationToken)))
            .Produces<IngestEventsResult>();
    }
}
