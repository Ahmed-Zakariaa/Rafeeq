using Microsoft.EntityFrameworkCore;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Identity.DTOs;
using Rafeeq.Domain.Identity.IService;
using Rafeeq.Infrastructure.Persistence;
using Rafeeq.Infrastructure.Security;

namespace Rafeeq.Application.Identity;

public class AdminRoleService : IAdminRoleService
{
    private readonly RafeeqDbContext _db;
    public AdminRoleService(RafeeqDbContext db) => _db = db;

    public async Task<ResultViewModel<List<AdminRoleDto>>> List()
    {
        var roles = await _db.AdminRoles.AsNoTracking()
            .Include(r => r.Permissions)
            .OrderBy(r => r.Name)
            .ToListAsync();

        var dtos = roles.Select(r => new AdminRoleDto
        {
            UniqueId = EncryptionHelper.EncryptToUrl(r.Id),
            Name = r.Name,
            Permissions = r.Permissions.Select(p => p.Permission).ToArray(),
        }).ToList();

        return ResultViewModel<List<AdminRoleDto>>.Success(dtos);
    }

    public async Task<ResultViewModel<bool>> Create(AdminRoleSaveDto dto)
    {
        ValidatePermissions(dto.Permissions);
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BusinessException("roleNameRequired");

        var role = new AdminRole(dto.Name);
        await _db.AdminRoles.AddAsync(role);
        await _db.SaveChangesAsync();

        foreach (var p in dto.Permissions.Distinct())
            _db.AdminRolePermissions.Add(new AdminRolePermission(role.Id, p));
        await _db.SaveChangesAsync();

        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> Update(string uniqueId, AdminRoleSaveDto dto)
    {
        ValidatePermissions(dto.Permissions);
        var id = EncryptionHelper.DecryptFromUrl(uniqueId);
        var role = await _db.AdminRoles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new BusinessException("roleNotFound");

        role.Rename(dto.Name);
        _db.AdminRolePermissions.RemoveRange(role.Permissions);
        foreach (var p in dto.Permissions.Distinct())
            _db.AdminRolePermissions.Add(new AdminRolePermission(role.Id, p));

        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> Delete(string uniqueId)
    {
        var id = EncryptionHelper.DecryptFromUrl(uniqueId);
        var role = await _db.AdminRoles.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new BusinessException("roleNotFound");

        _db.AdminRoles.Remove(role);   // cascades permissions + assignments
        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<List<string>>> GetUserRoles(string userUniqueId)
    {
        var userId = EncryptionHelper.DecryptFromUrl(userUniqueId);
        var roleIds = await _db.UserAdminRoles.AsNoTracking()
            .Where(ua => ua.UserId == userId).Select(ua => ua.AdminRoleId).ToListAsync();
        return ResultViewModel<List<string>>.Success(roleIds.Select(EncryptionHelper.EncryptToUrl).ToList());
    }

    public async Task<ResultViewModel<bool>> AssignUserRoles(string userUniqueId, AssignRolesDto dto)
    {
        var userId = EncryptionHelper.DecryptFromUrl(userUniqueId);

        var existing = await _db.UserAdminRoles.Where(ua => ua.UserId == userId).ToListAsync();
        _db.UserAdminRoles.RemoveRange(existing);

        foreach (var rid in dto.RoleUniqueIds.Distinct())
            _db.UserAdminRoles.Add(new UserAdminRole(userId, EncryptionHelper.DecryptFromUrl(rid)));

        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    private static void ValidatePermissions(IEnumerable<string> permissions)
    {
        if (permissions.Any(p => !Permissions.AdminCatalog.Contains(p)))
            throw new BusinessException("invalidPermission");
    }
}
