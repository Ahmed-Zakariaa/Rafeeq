using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity.DTOs;

namespace Rafeeq.Domain.Identity.IService;

public interface IAdminService
{
    Task<ResultViewModel<List<UserListItemDto>>> ListUsers(
        QueryViewModel<UserListFilterDto, EnumUserSortDto> query);

    Task<ResultViewModel<bool>> CreateAdmin(CreateAdminDto dto);
    Task<ResultViewModel<List<string>>> GetUserPermissions(string uniqueId);
    Task<ResultViewModel<bool>> AssignPermissions(string uniqueId, AssignPermissionsDto dto);
    Task<ResultViewModel<bool>> SetUserStatus(string uniqueId, SetUserStatusDto dto);
    Task<ResultViewModel<DashboardDto>> GetDashboard();
    Task<ResultViewModel<List<string>>> GetPermissionCatalog();
}
