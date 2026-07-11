using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;

namespace Rafeeq.Domain.Notifications;

/// <summary>An in-app notification for a user. TitleKey is an i18n key the FE translates; Body is optional context.</summary>
public class Notification : BaseEntity<int>
{
    public int UserId { get; private set; }
    public User? User { get; private set; }
    public NotificationType Type { get; private set; }
    public string TitleKey { get; private set; } = string.Empty;
    public string? Body { get; private set; }
    public bool IsRead { get; private set; }

    private Notification() { }

    public Notification(int userId, NotificationType type, string titleKey, string? body)
    {
        UserId = userId;
        Type = type;
        TitleKey = titleKey;
        Body = body;
    }

    public void MarkRead() => IsRead = true;
}
