using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Notifications;

namespace NotifyMe.Application.Tests.TestDoubles;

public sealed class InMemoryNotificationRepository : INotificationRepository
{
    private readonly Dictionary<Guid, Notification> _notifications = new();

    public IReadOnlyCollection<Notification> Saved => _notifications.Values;

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_notifications.GetValueOrDefault(id));

    public Task<IReadOnlyList<Notification>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Notification>>(_notifications.Values.ToList());

    public Task AddAsync(Notification notification, CancellationToken cancellationToken)
    {
        _notifications[notification.Id] = notification;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
    {
        _notifications[notification.Id] = notification;
        return Task.CompletedTask;
    }
}
