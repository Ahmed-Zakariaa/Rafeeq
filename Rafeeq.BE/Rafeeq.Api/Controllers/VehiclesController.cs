using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.Api.Authorization;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Vehicles.DTOs;
using Rafeeq.Domain.Vehicles.IService;

namespace Rafeeq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicles;
    public VehiclesController(IVehicleService vehicles) => _vehicles = vehicles;

    [HasPermission(Permissions.VehiclesManageOwn)]
    [HttpPost("Create")]
    public async Task<ActionResult<ResultViewModel<bool>>> Create(VehicleCreateDto dto)
        => Ok(await _vehicles.Create(dto));

    [HasPermission(Permissions.VehiclesManageOwn)]
    [HttpGet("Mine")]
    public async Task<ActionResult<ResultViewModel<List<VehicleResultDto>>>> Mine()
        => Ok(await _vehicles.GetMine());
}
