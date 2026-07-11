using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Rafeeq.Infrastructure.Persistence;

/// <summary>
/// Design-time factory so <c>dotnet ef migrations</c> can build the context without running the app.
/// The connection string here is only used to pick the provider; migrations don't touch a live DB.
/// </summary>
public class RafeeqDbContextFactory : IDesignTimeDbContextFactory<RafeeqDbContext>
{
    public RafeeqDbContext CreateDbContext(string[] args)
    {
        // Only the provider matters for generating migrations; set RAFEEQ_DB to run them (e.g. against Neon).
        var conn = Environment.GetEnvironmentVariable("RAFEEQ_DB")
            ?? "Host=localhost;Database=rafeeq;Username=postgres;Password=postgres";
        var options = new DbContextOptionsBuilder<RafeeqDbContext>()
            .UseNpgsql(conn)
            .Options;
        return new RafeeqDbContext(options);
    }
}
