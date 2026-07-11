using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity.DTOs;

namespace Rafeeq.Domain.Identity.IService;

public interface IAdminRoleService
{
    Task<ResultViewModel<List<AdminRoleDto>>> List();
    Task<ResultViewModel<bool>> Create(AdminRoleSaveDto dto);
    Task<ResultViewModel<bool>> Update(string uniqueId, AdminRoleSaveDto dto);
    Task<ResultViewModel<bool>> Delete(string uniqueId);

    Task<ResultViewModel<List<string>>> GetUserRoles(string userUniqueId);     // assigned role uniqueIds
    Task<ResultViewModel<bool>> AssignUserRoles(string userUniqueId, AssignRolesDto dto);
}
