using Microsoft.Extensions.Options;
using NotifyMe.Application.Events;
using NotifyMe.Infrastructure.EventSources.Simulated;

namespace NotifyMe.Api.Workers;

/// <summary>
/// Automates the pipeline described in ai/plan/phase-09-background-workers.md: polls
/// <see cref="IngestEventsUseCase"/> (which already fuses fetch -> normalize -> match -> dispatch
/// into a single call, see its own doc comment) on the interval configured for the event source,
/// so simulated events flow end-to-end without any manual `POST /api/admin/events/trigger-simulated`
/// call.
///
/// A separate "evaluation and dispatch" worker (as originally sketched in the phase plan) was
/// not added: that would just split one already-atomic use-case call across two hosted services
/// for no benefit, mirroring the course-correction already made in Phase 07 (no separate
/// `ChannelResolver` type, since `DispatchNotificationUseCase` already fills that role).
///
/// Runs one ingestion pass immediately on startup (rather than waiting a full interval first) so
/// the demo doesn't sit idle, then repeats on <see cref="SimulatedEventSourceOptions.PollingInterval"/>.
/// A failure on any single pass is logged and does not stop subsequent polling.
/// </summary>
public sealed class EventIngestionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SimulatedEventSourceOptions _options;
    private readonly ILogger<EventIngestionWorker> _logger;

    public EventIngestionWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<SimulatedEventSourceOptions> options,
        ILogger<EventIngestionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.PollingInterval);

        do
        {
            await RunOnceAsync(stoppingToken);
        }
        while (await WaitForNextTickAsync(timer, stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var useCase = scope.ServiceProvider.GetRequiredService<IngestEventsUseCase>();
            var result = await useCase.ExecuteAsync(stoppingToken);

            _logger.LogInformation(
                "Background ingestion tick: fetched {RawEventCount}, normalized {NormalizedEventCount}, dispatched {DispatchedCount}.",
                result.RawEventCount, result.NormalizedEventCount, result.NotificationsDispatched);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unhandled error during background event ingestion tick.");
        }
    }

    /// <summary>
    /// Small wrapper so a cancellation during shutdown ends the loop quietly instead of
    /// surfacing an <see cref="OperationCanceledException"/> out of <see cref="ExecuteAsync"/>.
    /// </summary>
    private static async Task<bool> WaitForNextTickAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
