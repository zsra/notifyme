using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Common.Exceptions;

namespace NotifyMe.Application.Notifications;

public sealed class GetNotificationUseCase
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationUseCase(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<NotificationDto> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Notification '{id}' was not found.");

        return NotificationDto.FromEntity(notification);
    }
}
