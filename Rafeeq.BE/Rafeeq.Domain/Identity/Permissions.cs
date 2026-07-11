using Rafeeq.Domain.Common;

namespace Rafeeq.Domain.Identity;

/// <summary>
/// Permission keys ("Resource.Action") used by [HasPermission] on endpoints.
/// Mirrors the RBAC matrix in docs/v1-spec.md §6.
/// </summary>
public static class Permissions
{
    // Trips
    public const string TripsCreate = "Trips.Create";
    public const string TripsView = "Trips.View";
    public const string TripsManageOwn = "Trips.ManageOwn";   // edit/cancel own
    public const string TripsForceCancel = "Trips.ForceCancel";

    // Bookings
    public const string BookingsRequest = "Bookings.Request";
    public const string BookingsRespond = "Bookings.Respond";  // accept/reject on own trip

    // Vehicles
    public const string VehiclesManageOwn = "Vehicles.ManageOwn";

    // Verification
    public const string VerificationUpload = "Verification.Upload";
    public const string VerificationReview = "Verification.Review";

    // Users / Reports / Ratings / Lookups / Dashboard
    public const string UsersView = "Users.View";
    public const string UsersManage = "Users.Manage";
    public const string ReportsCreate = "Reports.Create";
    public const string ReportsHandle = "Reports.Handle";
    public const string RatingsCreate = "Ratings.Create";
    public const string LookupsManage = "Lookups.Manage";
    public const string DashboardView = "Dashboard.View";

    // Admin-portal module management (dynamic per-admin grants; super admin gets all)
    public const string DriversView = "Drivers.View";
    public const string DriversManage = "Drivers.Manage";
    public const string PassengersView = "Passengers.View";
    public const string PassengersManage = "Passengers.Manage";
    public const string VehiclesView = "Vehicles.View";
    public const string AdminsView = "Admins.View";
    public const string AdminsManage = "Admins.Manage";   // create admins + assign their permissions

    /// <summary>
    /// The permissions a super admin can grant to an admin (also the full set a super admin holds).
    /// This is the source list for the assignment UI.
    /// </summary>
    public static readonly string[] AdminCatalog =
    {
        DashboardView,
        DriversView, DriversManage,
        PassengersView, PassengersManage,
        UsersView, UsersManage,
        VehiclesView,
        TripsView, TripsForceCancel,
        VerificationReview,
        ReportsHandle,
        LookupsManage,
        AdminsView, AdminsManage,
    };
}

/// <summary>Maps each role to the permissions it grants (seeded into JWT claims at login).</summary>
public static class RolePermissions
{
    public static IReadOnlyDictionary<RoleType, string[]> Map { get; } = new Dictionary<RoleType, string[]>
    {
        [RoleType.Driver] = new[]
        {
            Permissions.TripsCreate, Permissions.TripsView, Permissions.TripsManageOwn,
            Permissions.BookingsRespond, Permissions.VehiclesManageOwn,
            Permissions.VerificationUpload, Permissions.ReportsCreate, Permissions.RatingsCreate,
        },
        [RoleType.Passenger] = new[]
        {
            Permissions.TripsView, Permissions.BookingsRequest,
            Permissions.VerificationUpload, Permissions.ReportsCreate, Permissions.RatingsCreate,
        },
        [RoleType.Admin] = new[]
        {
            Permissions.TripsView, Permissions.TripsForceCancel, Permissions.VerificationReview,
            Permissions.UsersManage, Permissions.ReportsHandle, Permissions.LookupsManage,
            Permissions.DashboardView,
        },
    };

    public static IEnumerable<string> ForRoles(IEnumerable<RoleType> roles) =>
        roles.SelectMany(r => Map.TryGetValue(r, out var p) ? p : Array.Empty<string>()).Distinct();
}
