using Microsoft.EntityFrameworkCore;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Notifications;
using Rafeeq.Domain.Notifications.DTOs;
using Rafeeq.Domain.Notifications.IService;
using Rafeeq.Infrastructure.Persistence;
using Rafeeq.Infrastructure.Security;

namespace Rafeeq.Application.Notifications;

public class NotificationService : INotificationService
{
    private readonly RafeeqDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IEmailSender _email;

    public NotificationService(RafeeqDbContext db, ICurrentUser currentUser, IEmailSender email)
    {
        _db = db;
        _currentUser = currentUser;
        _email = email;
    }

    public async Task Notify(int userId, NotificationType type, string titleKey, string? body)
    {
        await _db.Notifications.AddAsync(new Notification(userId, type, titleKey, body));

        // Also email the user (free SMTP — logs in dev). Caller persists the in-app row.
        var address = await _db.Users.Where(u => u.Id == userId).Select(u => u.Email).FirstOrDefaultAsync();
        if (!string.IsNullOrEmpty(address))
            await _email.SendAsync(address, "Rafeeq", body is null ? titleKey : $"{titleKey} — {body}");
    }

    public async Task<ResultViewModel<List<NotificationDto>>> GetMine()
    {
        var userId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");

        var list = await _db.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .Take(50)
            .Select(n => new NotificationDto
            {
                UniqueId = EncryptionHelper.EncryptToUrl(n.Id),
                Type = n.Type.ToString(),
                TitleKey = n.TitleKey,
                Body = n.Body,
                IsRead = n.IsRead,
                CreatedDate = n.CreatedDate,
            })
            .ToListAsync();

        return ResultViewModel<List<NotificationDto>>.Success(list);
    }

    public async Task<ResultViewModel<int>> UnreadCount()
    {
        var userId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var count = await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        return ResultViewModel<int>.Success(count);
    }

    public async Task<ResultViewModel<bool>> MarkRead(string uniqueId)
    {
        var userId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var id = EncryptionHelper.DecryptFromUrl(uniqueId);

        var notif = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notif is not null)
        {
            notif.MarkRead();
            await _db.SaveChangesAsync();
        }
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> MarkAllRead()
    {
        var userId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var unread = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in unread) n.MarkRead();
        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }
}
