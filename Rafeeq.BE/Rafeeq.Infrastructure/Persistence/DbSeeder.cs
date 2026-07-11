using Microsoft.EntityFrameworkCore;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Geography;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Identity.IService;

namespace Rafeeq.Infrastructure.Persistence;

/// <summary>Applies migrations and seeds reference data (roles, EG+KSA geography, super admin).</summary>
public static class DbSeeder
{
    // Bootstrap super-admin. Dev-only temp password — change it after first login.
    public const string SuperAdminEmail = "ahmedzakaria617@gmail.com";
    private const string SuperAdminTempPassword = "Rafeeq@Super1";

    public static async Task SeedAsync(RafeeqDbContext db, IPasswordHasher hasher)
    {
        await db.Database.MigrateAsync();

        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(new Role(RoleType.Driver), new Role(RoleType.Passenger), new Role(RoleType.Admin));
            await db.SaveChangesAsync();
        }

        if (!await db.Countries.AnyAsync())
        {
            var egypt = new Country("مصر", "Egypt", "EG", "EGP", "+20");
            var ksa = new Country("السعودية", "Saudi Arabia", "SA", "SAR", "+966");
            db.Countries.AddRange(egypt, ksa);
            await db.SaveChangesAsync();

            db.Cities.AddRange(
                new City(egypt.Id, "القاهرة", "Cairo"),
                new City(egypt.Id, "الإسكندرية", "Alexandria"),
                new City(ksa.Id, "الرياض", "Riyadh"),
                new City(ksa.Id, "جدة", "Jeddah"),
                new City(ksa.Id, "الدمام", "Dammam"));
            await db.SaveChangesAsync();
        }

        if (!await db.Users.AnyAsync(u => u.Email == SuperAdminEmail))
        {
            var country = await db.Countries.OrderBy(c => c.Id).FirstAsync();
            var super = new User("Super Admin", SuperAdminEmail, "+20",
                hasher.Hash(SuperAdminTempPassword), Gender.Male, country.Id);
            super.MarkSuperAdmin();
            super.VerifyEmail();
            db.Users.Add(super);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole(super.Id, (int)RoleType.Admin));
            await db.SaveChangesAsync();

            Console.WriteLine($"[Seed] Super admin ready: {SuperAdminEmail} / {SuperAdminTempPassword} (change after first login)");
        }
    }
}
