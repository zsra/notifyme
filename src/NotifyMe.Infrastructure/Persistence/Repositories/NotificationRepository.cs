using Microsoft.EntityFrameworkCore;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Notifications;

namespace NotifyMe.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NotifyMeDbContext _dbContext;

    public NotificationRepository(NotifyMeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Notifications.FirstOrDefaultAsync(notification => notification.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Notification>> ListAsync(CancellationToken cancellationToken) =>
        await _dbContext.Notifications.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken)
    {
        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
    {
        _dbContext.Notifications.Update(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
