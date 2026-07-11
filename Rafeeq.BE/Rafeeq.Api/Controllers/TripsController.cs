using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.Api.Authorization;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Trips.DTOs;
using Rafeeq.Domain.Trips.IService;

namespace Rafeeq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly ITripService _trips;
    public TripsController(ITripService trips) => _trips = trips;

    [HasPermission(Permissions.TripsCreate)]
    [HttpPost("Create")]
    public async Task<ActionResult<ResultViewModel<bool>>> Create(TripCreateDto dto)
        => Ok(await _trips.Create(dto));

    [HasPermission(Permissions.TripsView)]
    [HttpPost("Search")]
    public async Task<ActionResult<ResultViewModel<List<TripResultDto>>>> Search(
        QueryViewModel<TripSearchFilterDto, EnumTripSortDto> query)
        => Ok(await _trips.Search(query));

    [HasPermission(Permissions.TripsManageOwn)]
    [HttpGet("Mine")]
    public async Task<ActionResult<ResultViewModel<List<TripResultDto>>>> Mine()
        => Ok(await _trips.GetMine());

    [HasPermission(Permissions.TripsView)]
    [HttpGet("GetOne/{uniqueId}")]
    public async Task<ActionResult<ResultViewModel<TripResultDto>>> GetOne(string uniqueId)
        => Ok(await _trips.GetOne(uniqueId));

    [HasPermission(Permissions.TripsManageOwn)]
    [HttpPost("Complete/{uniqueId}")]
    public async Task<ActionResult<ResultViewModel<bool>>> Complete(string uniqueId)
        => Ok(await _trips.Complete(uniqueId));

    [HasPermission(Permissions.TripsManageOwn)]
    [HttpDelete("Cancel/{uniqueId}")]
    public async Task<ActionResult<ResultViewModel<bool>>> Cancel(string uniqueId)
        => Ok(await _trips.Cancel(uniqueId));
}
