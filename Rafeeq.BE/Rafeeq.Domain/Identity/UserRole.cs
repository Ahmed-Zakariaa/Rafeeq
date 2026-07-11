namespace Rafeeq.Domain.Identity;

/// <summary>Join entity — a user may hold both Driver and Passenger roles.</summary>
public class UserRole
{
    public int UserId { get; private set; }
    public User? User { get; private set; }
    public int RoleId { get; private set; }
    public Role? Role { get; private set; }

    private UserRole() { }

    public UserRole(int userId, int roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}
