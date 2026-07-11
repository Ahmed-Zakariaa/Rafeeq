using Rafeeq.Domain.Common;

namespace Rafeeq.Domain.Identity;

/// <summary>
/// A single "Resource.Action" permission granted to one admin user. This is the dynamic,
/// per-admin RBAC layer (super admin assigns these) — distinct from the static role→permission
/// map that still serves Driver/Passenger. Granted permissions are baked into the admin's JWT.
/// </summary>
public class UserPermission : BaseEntity<int>
{
    public int UserId { get; private set; }
    public User? User { get; private set; }
    public string Permission { get; private set; } = string.Empty;

    private UserPermission() { }

    public UserPermission(int userId, string permission)
    {
        UserId = userId;
        Permission = permission;
    }
}
