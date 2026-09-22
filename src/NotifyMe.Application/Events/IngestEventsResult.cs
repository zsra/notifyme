namespace NotifyMe.Application.Events;

/// <summary>
/// Summary of one ingestion pass, mainly for logging/the demo "trigger simulated event" admin
/// endpoint (docs/api/admin-api.md) to report back what happened.
/// </summary>
public sealed record IngestEventsResult(int RawEventCount, int NormalizedEventCount, int NotificationsDispatched);
