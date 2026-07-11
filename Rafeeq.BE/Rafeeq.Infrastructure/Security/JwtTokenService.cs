using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Rafeeq.Domain.Identity.IService;

namespace Rafeeq.Infrastructure.Security;

public class JwtSettings
{
    public string Issuer { get; set; } = "Rafeeq";
    public string Audience { get; set; } = "RafeeqClient";
    public string SecretKey { get; set; } = "CHANGE_ME_dev_only_secret_key_at_least_32_chars_long!";
    public int ExpiryMinutes { get; set; } = 1440; // 24h
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    public JwtTokenService(JwtSettings settings) => _settings = settings;

    public TokenResult Generate(int userId, string fullName, IEnumerable<string> roles,
        IEnumerable<string> permissions, bool isSuperAdmin = false, bool emailVerified = false)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_settings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("fullName", fullName),
            new("isSuperAdmin", isSuperAdmin ? "true" : "false"),
            new("emailVerified", emailVerified ? "true" : "false"),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_settings.Issuer, _settings.Audience, claims, now, expires, creds);

        return new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
