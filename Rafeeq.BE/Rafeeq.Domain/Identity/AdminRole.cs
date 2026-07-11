using Rafeeq.Domain.Common;

namespace Rafeeq.Domain.Identity;

/// <summary>
/// A named permission set ("rule") a super admin defines and assigns to admins. An admin's effective
/// permissions = direct grants (UserPermission) ∪ permissions from their assigned AdminRoles.
/// </summary>
public class AdminRole : BaseEntity<int>
{
    public string Name { get; private set; } = string.Empty;
    public ICollection<AdminRolePermission> Permissions { get; private set; } = new List<AdminRolePermission>();

    private AdminRole() { }
    public AdminRole(string name) => Name = name.Trim();
    public void Rename(string name) => Name = name.Trim();
}

public class AdminRolePermission : BaseEntity<int>
{
    public int AdminRoleId { get; private set; }
    public AdminRole? AdminRole { get; private set; }
    public string Permission { get; private set; } = string.Empty;

    private AdminRolePermission() { }
    public AdminRolePermission(int adminRoleId, string permission)
    {
        AdminRoleId = adminRoleId;
        Permission = permission;
    }
}

/// <summary>Assignment of an AdminRole to a user.</summary>
public class UserAdminRole : BaseEntity<int>
{
    public int UserId { get; private set; }
    public int AdminRoleId { get; private set; }
    public AdminRole? AdminRole { get; private set; }

    private UserAdminRole() { }
    public UserAdminRole(int userId, int adminRoleId)
    {
        UserId = userId;
        AdminRoleId = adminRoleId;
    }
}
