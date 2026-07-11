using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.Api.Authorization;
using Rafeeq.Domain.Bookings.DTOs;
using Rafeeq.Domain.Bookings.IService;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;

namespace Rafeeq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookings;
    public BookingsController(IBookingService bookings) => _bookings = bookings;

    [HasPermission(Permissions.BookingsRequest)]
    [HttpPost("Request")]
    public async Task<ActionResult<ResultViewModel<bool>>> Request(BookingRequestDto dto)
        => Ok(await _bookings.Request(dto));

    [HasPermission(Permissions.BookingsRespond)]
    [HttpPost("Respond")]
    public async Task<ActionResult<ResultViewModel<bool>>> Respond(BookingRespondDto dto)
        => Ok(await _bookings.Respond(dto));

    [HasPermission(Permissions.BookingsRequest)]
    [HttpDelete("Cancel/{uniqueId}")]
    public async Task<ActionResult<ResultViewModel<bool>>> Cancel(string uniqueId)
        => Ok(await _bookings.Cancel(uniqueId));

    [HasPermission(Permissions.BookingsRequest)]
    [HttpGet("Mine")]
    public async Task<ActionResult<ResultViewModel<List<BookingResultDto>>>> Mine()
        => Ok(await _bookings.GetMine());

    [HasPermission(Permissions.BookingsRespond)]
    [HttpGet("Incoming")]
    public async Task<ActionResult<ResultViewModel<List<BookingResultDto>>>> Incoming()
        => Ok(await _bookings.GetIncoming());
}
