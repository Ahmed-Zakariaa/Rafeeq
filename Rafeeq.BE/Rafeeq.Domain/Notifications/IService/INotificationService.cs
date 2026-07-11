using Rafeeq.Domain.Common;
using Rafeeq.Domain.Notifications.DTOs;

namespace Rafeeq.Domain.Notifications.IService;

public interface INotificationService
{
    /// <summary>Create an in-app notification (+ email) for a user. Does not SaveChanges — the caller persists.</summary>
    Task Notify(int userId, NotificationType type, string titleKey, string? body);

    Task<ResultViewModel<List<NotificationDto>>> GetMine();
    Task<ResultViewModel<int>> UnreadCount();
    Task<ResultViewModel<bool>> MarkRead(string uniqueId);
    Task<ResultViewModel<bool>> MarkAllRead();
}
