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
        var options = new DbContextOptionsBuilder<RafeeqDbContext>()
            .UseSqlServer("Server=.;Database=RafeeqDb;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        return new RafeeqDbContext(options);
    }
}
