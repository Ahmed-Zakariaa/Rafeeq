using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.Api.Authorization;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Geography.DTOs;
using Rafeeq.Domain.Geography.IService;
using Rafeeq.Domain.Identity;

namespace Rafeeq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly ILookupService _lookups;
    public LookupsController(ILookupService lookups) => _lookups = lookups;

    // ── Public reads (register is anonymous; trip forms need cities) ──
    [AllowAnonymous]
    [HttpGet("Countries")]
    public async Task<ActionResult<ResultViewModel<List<CountryDto>>>> Countries()
        => Ok(await _lookups.GetCountries());

    [AllowAnonymous]
    [HttpGet("Cities")]
    public async Task<ActionResult<ResultViewModel<List<CityDto>>>> Cities([FromQuery] int? countryId)
        => Ok(await _lookups.GetCities(countryId));

    // ── Admin management (Lookups.Manage) ──
    [HasPermission(Permissions.LookupsManage)]
    [HttpGet("Admin/Countries")]
    public async Task<ActionResult<ResultViewModel<List<CountryDto>>>> AdminCountries()
        => Ok(await _lookups.GetCountries(includeInactive: true));

    [HasPermission(Permissions.LookupsManage)]
    [HttpPost("Countries")]
    public async Task<ActionResult<ResultViewModel<bool>>> CreateCountry(CountryUpsertDto dto)
        => Ok(await _lookups.CreateCountry(dto));

    [HasPermission(Permissions.LookupsManage)]
    [HttpPut("Countries/{id:int}")]
    public async Task<ActionResult<ResultViewModel<bool>>> UpdateCountry(int id, CountryUpsertDto dto)
        => Ok(await _lookups.UpdateCountry(id, dto));

    [HasPermission(Permissions.LookupsManage)]
    [HttpPost("Countries/{id:int}/Active")]
    public async Task<ActionResult<ResultViewModel<bool>>> SetCountryActive(int id, SetActiveDto dto)
        => Ok(await _lookups.SetCountryActive(id, dto.IsActive));

    [HasPermission(Permissions.LookupsManage)]
    [HttpGet("Admin/Cities")]
    public async Task<ActionResult<ResultViewModel<List<CityDto>>>> AdminCities([FromQuery] int? countryId)
        => Ok(await _lookups.GetCities(countryId, includeInactive: true));

    [HasPermission(Permissions.LookupsManage)]
    [HttpPost("Cities")]
    public async Task<ActionResult<ResultViewModel<bool>>> CreateCity(CityUpsertDto dto)
        => Ok(await _lookups.CreateCity(dto));

    [HasPermission(Permissions.LookupsManage)]
    [HttpPut("Cities/{id:int}")]
    public async Task<ActionResult<ResultViewModel<bool>>> UpdateCity(int id, CityUpsertDto dto)
        => Ok(await _lookups.UpdateCity(id, dto));

    [HasPermission(Permissions.LookupsManage)]
    [HttpPost("Cities/{id:int}/Active")]
    public async Task<ActionResult<ResultViewModel<bool>>> SetCityActive(int id, SetActiveDto dto)
        => Ok(await _lookups.SetCityActive(id, dto.IsActive));
}
