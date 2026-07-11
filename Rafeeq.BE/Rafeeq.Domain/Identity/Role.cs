using Rafeeq.Domain.Common;

namespace Rafeeq.Domain.Identity;

public class Role : BaseEntity<int>
{
    public string Name { get; private set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private Role() { }

    public Role(RoleType type)
    {
        Id = (int)type;
        Name = type.ToString();
    }
}
