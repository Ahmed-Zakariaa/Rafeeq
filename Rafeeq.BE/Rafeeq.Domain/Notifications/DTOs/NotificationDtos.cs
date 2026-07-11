namespace Rafeeq.Domain.Notifications.DTOs;

public class NotificationDto
{
    public string UniqueId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TitleKey { get; set; } = string.Empty;
    public string? Body { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
}
