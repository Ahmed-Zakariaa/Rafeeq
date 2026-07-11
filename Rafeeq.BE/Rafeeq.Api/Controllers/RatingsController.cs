using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.Api.Authorization;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Ratings.DTOs;
using Rafeeq.Domain.Ratings.IService;

namespace Rafeeq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratings;
    public RatingsController(IRatingService ratings) => _ratings = ratings;

    [HasPermission(Permissions.RatingsCreate)]
    [HttpPost("Rate")]
    public async Task<ActionResult<ResultViewModel<bool>>> Rate(RateDto dto)
        => Ok(await _ratings.Rate(dto));
}
