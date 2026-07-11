using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.Api.Authorization;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Identity.DTOs;
using Rafeeq.Domain.Identity.IService;

namespace Rafeeq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;
    public AdminController(IAdminService admin) => _admin = admin;

    [HasPermission(Permissions.UsersView)]
    [HttpPost("Users")]
    public async Task<ActionResult<ResultViewModel<List<UserListItemDto>>>> Users(
        QueryViewModel<UserListFilterDto, EnumUserSortDto> query)
        => Ok(await _admin.ListUsers(query));

    [HasPermission(Permissions.AdminsManage)]
    [HttpPost("Admins")]
    public async Task<ActionResult<ResultViewModel<bool>>> CreateAdmin(CreateAdminDto dto)
        => Ok(await _admin.CreateAdmin(dto));

    [HasPermission(Permissions.AdminsView)]
    [HttpGet("Users/{uniqueId}/Permissions")]
    public async Task<ActionResult<ResultViewModel<List<string>>>> GetPermissions(string uniqueId)
        => Ok(await _admin.GetUserPermissions(uniqueId));

    [HasPermission(Permissions.AdminsManage)]
    [HttpPost("Users/{uniqueId}/Permissions")]
    public async Task<ActionResult<ResultViewModel<bool>>> AssignPermissions(
        string uniqueId, AssignPermissionsDto dto)
        => Ok(await _admin.AssignPermissions(uniqueId, dto));

    [HasPermission(Permissions.UsersManage)]
    [HttpPost("Users/{uniqueId}/Status")]
    public async Task<ActionResult<ResultViewModel<bool>>> SetStatus(
        string uniqueId, SetUserStatusDto dto)
        => Ok(await _admin.SetUserStatus(uniqueId, dto));

    [HasPermission(Permissions.DashboardView)]
    [HttpGet("Dashboard")]
    public async Task<ActionResult<ResultViewModel<DashboardDto>>> Dashboard()
        => Ok(await _admin.GetDashboard());

    [HasPermission(Permissions.AdminsManage)]
    [HttpGet("Permissions/Catalog")]
    public async Task<ActionResult<ResultViewModel<List<string>>>> Catalog()
        => Ok(await _admin.GetPermissionCatalog());
}
