using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.Api.Authorization;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Identity.DTOs;
using Rafeeq.Domain.Identity.IService;

namespace Rafeeq.Api.Controllers;

[ApiController]
[Route("api/AdminRoles")]
[Authorize]
[HasPermission(Permissions.AdminsManage)]
public class AdminRolesController : ControllerBase
{
    private readonly IAdminRoleService _roles;
    public AdminRolesController(IAdminRoleService roles) => _roles = roles;

    [HttpGet]
    public async Task<ActionResult<ResultViewModel<List<AdminRoleDto>>>> List()
        => Ok(await _roles.List());

    [HttpPost]
    public async Task<ActionResult<ResultViewModel<bool>>> Create(AdminRoleSaveDto dto)
        => Ok(await _roles.Create(dto));

    [HttpPut("{uniqueId}")]
    public async Task<ActionResult<ResultViewModel<bool>>> Update(string uniqueId, AdminRoleSaveDto dto)
        => Ok(await _roles.Update(uniqueId, dto));

    [HttpDelete("{uniqueId}")]
    public async Task<ActionResult<ResultViewModel<bool>>> Delete(string uniqueId)
        => Ok(await _roles.Delete(uniqueId));

    [HttpGet("User/{userUniqueId}")]
    public async Task<ActionResult<ResultViewModel<List<string>>>> GetUserRoles(string userUniqueId)
        => Ok(await _roles.GetUserRoles(userUniqueId));

    [HttpPost("User/{userUniqueId}")]
    public async Task<ActionResult<ResultViewModel<bool>>> AssignUserRoles(string userUniqueId, AssignRolesDto dto)
        => Ok(await _roles.AssignUserRoles(userUniqueId, dto));
}
