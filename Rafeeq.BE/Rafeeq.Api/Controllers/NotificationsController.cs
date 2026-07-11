using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Notifications.DTOs;
using Rafeeq.Domain.Notifications.IService;

namespace Rafeeq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]   // any authenticated user; notifications are personal
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;
    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    [HttpGet("Mine")]
    public async Task<ActionResult<ResultViewModel<List<NotificationDto>>>> Mine()
        => Ok(await _notifications.GetMine());

    [HttpGet("UnreadCount")]
    public async Task<ActionResult<ResultViewModel<int>>> UnreadCount()
        => Ok(await _notifications.UnreadCount());

    [HttpPost("MarkRead/{uniqueId}")]
    public async Task<ActionResult<ResultViewModel<bool>>> MarkRead(string uniqueId)
        => Ok(await _notifications.MarkRead(uniqueId));

    [HttpPost("MarkAllRead")]
    public async Task<ActionResult<ResultViewModel<bool>>> MarkAllRead()
        => Ok(await _notifications.MarkAllRead());
}
