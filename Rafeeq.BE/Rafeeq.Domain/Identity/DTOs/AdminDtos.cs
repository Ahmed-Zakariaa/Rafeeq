using Rafeeq.Domain.Common;

namespace Rafeeq.Domain.Identity.DTOs;

public class CreateAdminDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public int CountryId { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class AssignPermissionsDto
{
    public List<string> Permissions { get; set; } = new();
}

public class SetUserStatusDto
{
    public AccountStatus Status { get; set; }
}

public class UserListItemDto
{
    public string UniqueId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public string AccountStatus { get; set; } = string.Empty;
    public string VerificationLevel { get; set; } = string.Empty;
    public bool IsActivated { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class UserListFilterDto
{
    public string? Role { get; set; }          // "Driver" | "Passenger" | "Admin"
    public string? Search { get; set; }        // name / email / phone
    public AccountStatus? AccountStatus { get; set; }
}

public enum EnumUserSortDto { CreatedDate = 1, FullName = 2 }

public class DashboardDto
{
    public int TotalUsers { get; set; }
    public int Drivers { get; set; }
    public int Passengers { get; set; }
    public int Admins { get; set; }
    public int TotalTrips { get; set; }
    public int PublishedTrips { get; set; }
    public int TotalBookings { get; set; }
    public int TotalVehicles { get; set; }
}
