using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Identity.DTOs;
using Rafeeq.Domain.Identity.IService;
using Rafeeq.Infrastructure.Persistence;
using Rafeeq.Infrastructure.Security;

namespace Rafeeq.Application.Identity;

public class AdminService : IAdminService
{
    private readonly RafeeqDbContext _db;
    private readonly IEmailSender _email;
    private readonly string _frontendBaseUrl;

    public AdminService(RafeeqDbContext db, IEmailSender email, IConfiguration config)
    {
        _db = db;
        _email = email;
        _frontendBaseUrl = config["App:FrontendBaseUrl"] ?? "http://localhost:4200";
    }

    public async Task<ResultViewModel<List<UserListItemDto>>> ListUsers(
        QueryViewModel<UserListFilterDto, EnumUserSortDto> query)
    {
        var f = query.FilterModel;

        // Super admin is never listed.
        var q = _db.Users.AsNoTracking().Include(u => u.UserRoles).Where(u => !u.IsSuperAdmin);

        if (!string.IsNullOrWhiteSpace(f?.Role) && Enum.TryParse<RoleType>(f.Role, out var role))
            q = q.Where(u => u.UserRoles.Any(r => r.RoleId == (int)role));

        if (!string.IsNullOrWhiteSpace(f?.Search))
        {
            var s = f.Search.Trim();
            q = q.Where(u => u.FullName.Contains(s) || u.Email.Contains(s) || u.PhoneNumber.Contains(s));
        }

        q = q.WhereIf(f?.AccountStatus != null, u => u.AccountStatus == f!.AccountStatus);

        var sortField = query.OrderModel?.FieldName;
        var sortType = query.OrderModel?.SortType ?? SortType.Desc;
        q = q.SortIf(sortField is null or EnumUserSortDto.CreatedDate, u => u.CreatedDate, sortType);
        q = q.SortIf(sortField == EnumUserSortDto.FullName, u => u.FullName, sortType);

        var total = await q.CountAsync();
        var page = await q.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

        var items = page.Select(u => new UserListItemDto
        {
            UniqueId = EncryptionHelper.EncryptToUrl(u.Id),
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Roles = u.UserRoles.Select(r => ((RoleType)r.RoleId).ToString()).ToArray(),
            AccountStatus = u.AccountStatus.ToString(),
            VerificationLevel = u.VerificationLevel.ToString(),
            IsActivated = u.IsActivated,
            CreatedDate = u.CreatedDate,
        }).ToList();

        return ResultViewModel<List<UserListItemDto>>.Paged(items, total, query.PageNumber, query.PageSize);
    }

    public async Task<ResultViewModel<bool>> CreateAdmin(CreateAdminDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new BusinessException("emailAlreadyRegistered", nameof(dto.Email));
        if (!await _db.Countries.AnyAsync(c => c.Id == dto.CountryId))
            throw new BusinessException("countryNotFound", nameof(dto.CountryId));
        ValidatePermissions(dto.Permissions);

        var user = User.CreateInvited(dto.FullName, email, dto.PhoneNumber, dto.Gender, dto.CountryId);
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();

        _db.UserRoles.Add(new UserRole(user.Id, (int)RoleType.Admin));
        foreach (var p in dto.Permissions.Distinct())
            _db.UserPermissions.Add(new UserPermission(user.Id, p));

        var token = NewToken();
        user.SetPasswordResetToken(token, DateTime.UtcNow.AddHours(48));
        await _db.SaveChangesAsync();

        var link = $"{_frontendBaseUrl}/auth/set-password?token={token}";
        await _email.SendAsync(user.Email, "Your Rafeeq admin account",
            $"An admin account was created for you. Set your password to activate: {link}");

        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<List<string>>> GetUserPermissions(string uniqueId)
    {
        var user = await FindUser(uniqueId);
        var perms = await _db.UserPermissions.AsNoTracking()
            .Where(p => p.UserId == user.Id).Select(p => p.Permission).ToListAsync();
        return ResultViewModel<List<string>>.Success(perms);
    }

    public async Task<ResultViewModel<bool>> AssignPermissions(string uniqueId, AssignPermissionsDto dto)
    {
        var user = await FindUser(uniqueId);
        if (!await _db.UserRoles.AnyAsync(r => r.UserId == user.Id && r.RoleId == (int)RoleType.Admin))
            throw new BusinessException("permissionsOnlyForAdmins");
        ValidatePermissions(dto.Permissions);

        var existing = await _db.UserPermissions.Where(p => p.UserId == user.Id).ToListAsync();
        _db.UserPermissions.RemoveRange(existing);
        foreach (var p in dto.Permissions.Distinct())
            _db.UserPermissions.Add(new UserPermission(user.Id, p));

        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> SetUserStatus(string uniqueId, SetUserStatusDto dto)
    {
        var user = await FindUser(uniqueId);
        switch (dto.Status)
        {
            case AccountStatus.Active: user.Reactivate(); break;
            case AccountStatus.Suspended: user.Suspend(); break;
            case AccountStatus.Banned: user.Ban(); break;
            default: throw new BusinessException("invalidStatus");
        }
        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<DashboardDto>> GetDashboard()
    {
        var dto = new DashboardDto
        {
            TotalUsers = await _db.Users.CountAsync(u => !u.IsSuperAdmin),
            Drivers = await _db.UserRoles.CountAsync(r => r.RoleId == (int)RoleType.Driver),
            Passengers = await _db.UserRoles.CountAsync(r => r.RoleId == (int)RoleType.Passenger),
            Admins = await _db.Users.CountAsync(u => !u.IsSuperAdmin
                && u.UserRoles.Any(r => r.RoleId == (int)RoleType.Admin)),
            TotalTrips = await _db.Trips.CountAsync(),
            PublishedTrips = await _db.Trips.CountAsync(t => t.Status == TripStatus.Published),
            TotalBookings = await _db.Bookings.CountAsync(),
            TotalVehicles = await _db.Vehicles.CountAsync(),
        };
        return ResultViewModel<DashboardDto>.Success(dto);
    }

    public Task<ResultViewModel<List<string>>> GetPermissionCatalog()
        => Task.FromResult(ResultViewModel<List<string>>.Success(Permissions.AdminCatalog.ToList()));

    private async Task<User> FindUser(string uniqueId)
    {
        var id = EncryptionHelper.DecryptFromUrl(uniqueId);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new BusinessException("userNotFound");
        if (user.IsSuperAdmin)
            throw new BusinessException("cannotManageSuperAdmin"); // super admin is untouchable/hidden
        return user;
    }

    private static void ValidatePermissions(IEnumerable<string> permissions)
    {
        if (permissions.Any(p => !Permissions.AdminCatalog.Contains(p)))
            throw new BusinessException("invalidPermission");
    }

    private static string NewToken() => Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
}
