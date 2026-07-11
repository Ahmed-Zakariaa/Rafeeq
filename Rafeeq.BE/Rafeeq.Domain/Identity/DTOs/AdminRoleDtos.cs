namespace Rafeeq.Domain.Identity.DTOs;

public class AdminRoleDto
{
    public string UniqueId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = System.Array.Empty<string>();
}

public class AdminRoleSaveDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class AssignRolesDto
{
    public List<string> RoleUniqueIds { get; set; } = new();
}
