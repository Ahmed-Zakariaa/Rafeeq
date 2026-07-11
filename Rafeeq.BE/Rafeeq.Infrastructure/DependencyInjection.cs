using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity.IService;
using Rafeeq.Infrastructure.Email;
using Rafeeq.Infrastructure.Identity;
using Rafeeq.Infrastructure.Persistence;
using Rafeeq.Infrastructure.Security;
using Rafeeq.Infrastructure.Storage;

namespace Rafeeq.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<RafeeqDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("Default")));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        var jwt = config.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
        services.AddSingleton(jwt);

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        var emailSettings = config.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
        services.AddSingleton(emailSettings);
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IFileStorage, LocalFileStorage>();

        return services;
    }
}
