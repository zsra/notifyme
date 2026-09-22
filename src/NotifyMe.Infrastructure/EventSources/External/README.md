# Adding a real event source

This folder exists so the event-ingestion extension seam is discoverable in the codebase itself,
not just described in [docs/architecture/event-ingestion.md](../../../../docs/architecture/event-ingestion.md)
and [ADR-0004](../../../../ai/decisions/adr/0004-event-source-strategy.md). There is no real
external source implemented yet (see the "Known limitation" section of that doc) - this is a
guide for adding one later.

## Steps to add a real source

1. Implement `NotifyMe.Domain.Abstractions.IEventSource` in a new class under this folder, e.g.
   `NewsApiEventSource`, `EarthquakeFeedEventSource`, or `MarketDataEventSource`:

   ```csharp
   public sealed class NewsApiEventSource : IEventSource
   {
       public Task<IReadOnlyList<RawEvent>> FetchAsync(CancellationToken cancellationToken)
       {
           // Call the external API, map each result into RawEvent.Create(...), return the list.
       }
   }
   ```

2. Map whatever shape the external API returns into the same `RawEvent` shape the rest of the
   pipeline already expects (`Id`, `Source`, `EventCategory`, raw payload string, `OccurredAt`).
   Any parsing/HTTP-client/auth concerns belong entirely inside this class or its private
   collaborators - `Domain` and `Application` must not know these details exist.

3. Register the new implementation in DI, in place of (or alongside, if you want both) the
   `AddSimulatedEventSource` registration in
   `EventSources/EventSourcesServiceCollectionExtensions.cs`, wired up from the `Api` composition
   root. Add any API keys/credentials via `dotnet user-secrets`/environment variables, never
   committed to the repo.

4. No changes to `Domain`, `Application`, the alert-matching logic, or the notification channels
   should be required. If adding a source forces a change there, that's a sign the `IEventSource`
   abstraction has leaked and should be revisited (and probably warrants a new ADR).

## What NOT to do

Do not make `SimulatedEventSource` "smarter" to fake being a real source, and do not add
real-source-specific types (DTOs, HTTP clients, parsing helpers) to `Domain`/`Application`. Keep
the seam exactly at `IEventSource`.
