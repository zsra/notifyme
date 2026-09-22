using NotifyMe.Domain.Notifications;

namespace NotifyMe.Application.Abstractions.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Backs the Admin API's read-only notification history endpoint (see
    /// docs/api/admin-api.md). Filtering by status/alert rule/date range is added once that
    /// endpoint is actually implemented (Phase 08) rather than speculatively here.
    /// </summary>
    Task<IReadOnlyList<Notification>> ListAsync(CancellationToken cancellationToken);

    Task AddAsync(Notification notification, CancellationToken cancellationToken);

    Task UpdateAsync(Notification notification, CancellationToken cancellationToken);
}
