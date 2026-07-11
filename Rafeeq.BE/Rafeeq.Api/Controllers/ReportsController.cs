using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.Api.Authorization;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Reports.DTOs;
using Rafeeq.Domain.Reports.IService;

namespace Rafeeq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;
    public ReportsController(IReportService reports) => _reports = reports;

    [HasPermission(Permissions.ReportsCreate)]
    [HttpPost("Create")]
    public async Task<ActionResult<ResultViewModel<bool>>> Create(ReportCreateDto dto)
        => Ok(await _reports.Create(dto));

    [HasPermission(Permissions.ReportsHandle)]
    [HttpGet("All")]
    public async Task<ActionResult<ResultViewModel<List<ReportListItemDto>>>> All([FromQuery] ReportStatus? status)
        => Ok(await _reports.GetAll(status));

    [HasPermission(Permissions.ReportsHandle)]
    [HttpPost("Handle")]
    public async Task<ActionResult<ResultViewModel<bool>>> Handle(ReportHandleDto dto)
        => Ok(await _reports.Handle(dto));
}
