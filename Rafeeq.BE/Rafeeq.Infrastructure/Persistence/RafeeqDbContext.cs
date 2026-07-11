using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Rafeeq.Domain.Bookings;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Geography;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Notifications;
using Rafeeq.Domain.Ratings;
using Rafeeq.Domain.Reports;
using Rafeeq.Domain.Trips;
using Rafeeq.Domain.Vehicles;
using Rafeeq.Domain.Verification;

namespace Rafeeq.Infrastructure.Persistence;

public class RafeeqDbContext : DbContext
{
    private readonly ICurrentUser? _currentUser;

    public RafeeqDbContext(DbContextOptions<RafeeqDbContext> options, ICurrentUser? currentUser = null)
        : base(options) => _currentUser = currentUser;

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<AdminRole> AdminRoles => Set<AdminRole>();
    public DbSet<AdminRolePermission> AdminRolePermissions => Set<AdminRolePermission>();
    public DbSet<UserAdminRole> UserAdminRoles => Set<UserAdminRole>();
    public DbSet<VerificationDocument> VerificationDocuments => Set<VerificationDocument>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Join table
        b.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
        b.Entity<UserRole>().HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId);
        b.Entity<UserRole>().HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId);

        // Role ids are fixed (= RoleType value), not auto-generated
        b.Entity<Role>().Property(x => x.Id).ValueGeneratedNever();

        // Indexes
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Country>().HasIndex(x => x.IsoCode).IsUnique();

        // Per-admin permission grants
        b.Entity<UserPermission>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<UserPermission>().HasIndex(x => new { x.UserId, x.Permission }).IsUnique();

        // Admin roles (named permission sets) + assignments
        b.Entity<AdminRolePermission>().HasOne(p => p.AdminRole).WithMany(r => r.Permissions)
            .HasForeignKey(p => p.AdminRoleId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<AdminRolePermission>().HasIndex(p => new { p.AdminRoleId, p.Permission }).IsUnique();
        b.Entity<UserAdminRole>().HasOne(ua => ua.AdminRole).WithMany()
            .HasForeignKey(ua => ua.AdminRoleId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<UserAdminRole>().HasOne<User>().WithMany().HasForeignKey(ua => ua.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<UserAdminRole>().HasIndex(ua => new { ua.UserId, ua.AdminRoleId }).IsUnique();

        // Verification documents
        b.Entity<VerificationDocument>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<VerificationDocument>().HasIndex(x => x.UserId);

        // Reports (two user FKs + optional trip; restrict to avoid cascade paths)
        b.Entity<Report>().HasOne(r => r.Reporter).WithMany().HasForeignKey(r => r.ReporterUserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Report>().HasOne(r => r.Reported).WithMany().HasForeignKey(r => r.ReportedUserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Report>().HasOne(r => r.Trip).WithMany().HasForeignKey(r => r.TripId).OnDelete(DeleteBehavior.Restrict);

        // Ratings (two user FKs + trip; one rating per direction per trip)
        b.Entity<Rating>().HasOne(r => r.FromUser).WithMany().HasForeignKey(r => r.FromUserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Rating>().HasOne(r => r.ToUser).WithMany().HasForeignKey(r => r.ToUserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Rating>().HasOne(r => r.Trip).WithMany().HasForeignKey(r => r.TripId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Rating>().HasIndex(r => new { r.TripId, r.FromUserId, r.ToUserId }).IsUnique();

        // Notifications
        b.Entity<Notification>().HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Notification>().HasIndex(n => new { n.UserId, n.IsRead });

        // Money precision
        b.Entity<Trip>().Property(x => x.PricePerSeat).HasPrecision(10, 2);
        b.Entity<Booking>().Property(x => x.AgreedPrice).HasPrecision(10, 2);

        // Restrict deletes to avoid multiple cascade paths (SQL Server)
        b.Entity<Trip>().HasOne(t => t.OriginCity).WithMany().HasForeignKey(t => t.OriginCityId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Trip>().HasOne(t => t.DestinationCity).WithMany().HasForeignKey(t => t.DestinationCityId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Trip>().HasOne(t => t.Driver).WithMany().HasForeignKey(t => t.DriverId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Trip>().HasOne(t => t.Vehicle).WithMany().HasForeignKey(t => t.VehicleId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Booking>().HasOne(x => x.Trip).WithMany(t => t.Bookings).HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Booking>().HasOne(x => x.Passenger).WithMany().HasForeignKey(x => x.PassengerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Vehicle>().HasOne(v => v.User).WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<City>().HasOne(c => c.Country).WithMany(c => c.Cities).HasForeignKey(c => c.CountryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<User>().HasOne(u => u.Country).WithMany().HasForeignKey(u => u.CountryId).OnDelete(DeleteBehavior.Restrict);

        // Global soft-delete filter for every ICanBeSoftDeleted entity
        foreach (var et in b.Model.GetEntityTypes())
        {
            if (typeof(ICanBeSoftDeleted).IsAssignableFrom(et.ClrType))
            {
                var p = Expression.Parameter(et.ClrType, "e");
                var body = Expression.Not(Expression.Property(p, nameof(ICanBeSoftDeleted.IsDeleted)));
                b.Entity(et.ClrType).HasQueryFilter(Expression.Lambda(body, p));
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var uid = _currentUser?.UserId;
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
                entry.Entity.CreatedBy = uid;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedDate = now;
                entry.Entity.ModifiedBy = uid;
            }
        }
        return base.SaveChangesAsync(ct);
    }
}
