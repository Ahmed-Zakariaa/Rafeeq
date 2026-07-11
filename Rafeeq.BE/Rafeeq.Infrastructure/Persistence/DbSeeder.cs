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
            db.Countries.AddRange(
                new Country("مصر", "Egypt", "EG", "EGP", "+20"),
                new Country("السعودية", "Saudi Arabia", "SA", "SAR", "+966"));
            await db.SaveChangesAsync();
        }

        // Idempotent — adds any missing cities, so existing DBs pick up new ones on restart.
        await EnsureCities(db);

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

    private static async Task EnsureCities(RafeeqDbContext db)
    {
        var egyptId = await db.Countries.Where(c => c.IsoCode == "EG").Select(c => c.Id).FirstOrDefaultAsync();
        var ksaId = await db.Countries.Where(c => c.IsoCode == "SA").Select(c => c.Id).FirstOrDefaultAsync();
        if (egyptId == 0 || ksaId == 0) return;

        var desired = new List<(int CountryId, string Ar, string En)>
        {
            // Egypt
            (egyptId, "القاهرة", "Cairo"), (egyptId, "الإسكندرية", "Alexandria"), (egyptId, "الجيزة", "Giza"),
            (egyptId, "بورسعيد", "Port Said"), (egyptId, "السويس", "Suez"), (egyptId, "الأقصر", "Luxor"),
            (egyptId, "أسوان", "Aswan"), (egyptId, "المنصورة", "Mansoura"), (egyptId, "طنطا", "Tanta"),
            (egyptId, "أسيوط", "Asyut"), (egyptId, "الإسماعيلية", "Ismailia"), (egyptId, "الزقازيق", "Zagazig"),
            (egyptId, "الغردقة", "Hurghada"), (egyptId, "شرم الشيخ", "Sharm El Sheikh"), (egyptId, "6 أكتوبر", "6th of October"),
            // Saudi Arabia
            (ksaId, "الرياض", "Riyadh"), (ksaId, "جدة", "Jeddah"), (ksaId, "مكة المكرمة", "Mecca"),
            (ksaId, "المدينة المنورة", "Medina"), (ksaId, "الدمام", "Dammam"), (ksaId, "الخبر", "Khobar"),
            (ksaId, "الظهران", "Dhahran"), (ksaId, "الطائف", "Taif"), (ksaId, "تبوك", "Tabuk"),
            (ksaId, "بريدة", "Buraidah"), (ksaId, "خميس مشيط", "Khamis Mushait"), (ksaId, "أبها", "Abha"),
            (ksaId, "حائل", "Hail"), (ksaId, "الجبيل", "Jubail"), (ksaId, "ينبع", "Yanbu"),
        };

        var existing = (await db.Cities.Select(c => new { c.CountryId, c.NameEn }).ToListAsync())
            .Select(x => (x.CountryId, x.NameEn)).ToHashSet();

        var toAdd = desired
            .Where(d => !existing.Contains((d.CountryId, d.En)))
            .Select(d => new City(d.CountryId, d.Ar, d.En))
            .ToList();

        if (toAdd.Count > 0)
        {
            db.Cities.AddRange(toAdd);
            await db.SaveChangesAsync();
            Console.WriteLine($"[Seed] Added {toAdd.Count} cities (total catalog now ~{existing.Count + toAdd.Count}).");
        }
    }
}
