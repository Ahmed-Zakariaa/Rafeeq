using Microsoft.EntityFrameworkCore;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Identity.DTOs;
using Rafeeq.Domain.Identity.IService;
using Rafeeq.Infrastructure.Persistence;

namespace Rafeeq.Application.Identity;

public class AuthService : IAuthService
{
    // Where the FE serves the set-password / reset pages (dev). Move to config before deploy.
    private const string FrontendBaseUrl = "http://localhost:4200";

    private readonly RafeeqDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly IEmailSender _email;
    private readonly ICurrentUser _currentUser;

    public AuthService(RafeeqDbContext db, IPasswordHasher hasher, IJwtTokenService jwt,
        IEmailSender email, ICurrentUser currentUser)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _email = email;
        _currentUser = currentUser;
    }

    public async Task<ResultViewModel<AuthResultDto>> Register(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new BusinessException("emailAlreadyRegistered", nameof(dto.Email));
        if (!await _db.Countries.AnyAsync(c => c.Id == dto.CountryId))
            throw new BusinessException("countryNotFound", nameof(dto.CountryId));

        var user = new User(dto.FullName, email, dto.PhoneNumber, _hasher.Hash(dto.Password), dto.Gender, dto.CountryId);
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();

        // Everyone is a Passenger; optionally also a Driver.
        _db.UserRoles.Add(new UserRole(user.Id, (int)RoleType.Passenger));
        if (dto.AsDriver)
            _db.UserRoles.Add(new UserRole(user.Id, (int)RoleType.Driver));

        var code = NewOtp();
        user.SetEmailOtp(code, DateTime.UtcNow.AddMinutes(15));
        await _db.SaveChangesAsync();

        await _email.SendAsync(user.Email, "Rafeeq verification code", $"Your verification code is: {code}");

        return ResultViewModel<AuthResultDto>.Success(await BuildAuth(user));
    }

    public async Task<ResultViewModel<AuthResultDto>> VerifyEmail(VerifyEmailDto dto)
    {
        var userId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new BusinessException("userNotFound");

        if (!user.IsEmailVerified)
        {
            if (!user.ConfirmEmail(dto.Code.Trim()))
                throw new BusinessException("invalidOrExpiredOtp");
            await _db.SaveChangesAsync();
        }

        return ResultViewModel<AuthResultDto>.Success(await BuildAuth(user));
    }

    public async Task<ResultViewModel<bool>> ResendOtp()
    {
        var userId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new BusinessException("userNotFound");

        if (user.IsEmailVerified) return ResultViewModel<bool>.Success(true);

        var code = NewOtp();
        user.SetEmailOtp(code, DateTime.UtcNow.AddMinutes(15));
        await _db.SaveChangesAsync();

        await _email.SendAsync(user.Email, "Rafeeq verification code", $"Your verification code is: {code}");
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<AuthResultDto>> Login(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new BusinessException("invalidCredentials");

        if (!user.IsActivated)
            throw new BusinessException("accountNotActivated");
        if (!_hasher.Verify(dto.Password, user.PasswordHash))
            throw new BusinessException("invalidCredentials");
        if (user.AccountStatus != AccountStatus.Active)
            throw new BusinessException("accountNotActive");

        return ResultViewModel<AuthResultDto>.Success(await BuildAuth(user));
    }

    public async Task<ResultViewModel<bool>> ForgotPassword(ForgotPasswordDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Always return success — never reveal whether an email exists.
        if (user != null && user.AccountStatus != AccountStatus.Banned)
        {
            var token = NewToken();
            user.SetPasswordResetToken(token, DateTime.UtcNow.AddHours(2));
            await _db.SaveChangesAsync();

            var link = $"{FrontendBaseUrl}/auth/reset-password?token={token}";
            await _email.SendAsync(user.Email, "Reset your Rafeeq password",
                $"Use this link to reset your password: {link}");
        }

        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token);
        if (user == null || user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            throw new BusinessException("invalidOrExpiredToken");

        user.CompletePasswordSet(_hasher.Hash(dto.NewPassword));
        await _db.SaveChangesAsync();

        return ResultViewModel<bool>.Success(true);
    }

    private static string NewToken() => Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

    private static string NewOtp() =>
        System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

    private async Task<AuthResultDto> BuildAuth(User user)
    {
        var roleIds = await _db.UserRoles.Where(r => r.UserId == user.Id).Select(r => r.RoleId).ToListAsync();
        var roles = roleIds.Select(id => (RoleType)id).ToList();
        var roleNames = roles.Select(r => r.ToString()).ToArray();

        IEnumerable<string> permissions;
        if (user.IsSuperAdmin)
        {
            permissions = Permissions.AdminCatalog; // super admin holds everything grantable
        }
        else
        {
            // Driver/Passenger: static role map. Admin: per-admin DB grants (dynamic RBAC).
            var set = new HashSet<string>(RolePermissions.ForRoles(roles.Where(r => r != RoleType.Admin)));
            if (roles.Contains(RoleType.Admin))
            {
                // Admin effective permissions = direct grants ∪ permissions from assigned admin-roles.
                var granted = await _db.UserPermissions
                    .Where(p => p.UserId == user.Id).Select(p => p.Permission).ToListAsync();
                set.UnionWith(granted);

                var roleGranted = await _db.UserAdminRoles
                    .Where(ua => ua.UserId == user.Id)
                    .SelectMany(ua => ua.AdminRole!.Permissions.Select(p => p.Permission))
                    .ToListAsync();
                set.UnionWith(roleGranted);
            }
            permissions = set;
        }

        var token = _jwt.Generate(user.Id, user.FullName, roleNames, permissions, user.IsSuperAdmin, user.IsEmailVerified);
        return new AuthResultDto
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            FullName = user.FullName,
            Roles = roleNames,
            IsSuperAdmin = user.IsSuperAdmin,
            IsEmailVerified = user.IsEmailVerified,
        };
    }
}
