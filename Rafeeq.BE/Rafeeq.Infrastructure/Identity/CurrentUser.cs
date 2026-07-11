using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Rafeeq.Domain.Common;

namespace Rafeeq.Infrastructure.Identity;

/// <summary>Reads the authenticated user / language off the current HTTP request.</summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;
    public CurrentUser(IHttpContextAccessor http) => _http = http;

    public int? UserId
    {
        get
        {
            var value = _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string Language
    {
        get
        {
            var lang = _http.HttpContext?.Request.Headers.AcceptLanguage.ToString();
            return !string.IsNullOrWhiteSpace(lang) && lang.StartsWith("en", StringComparison.OrdinalIgnoreCase)
                ? "en" : "ar"; // Arabic default
        }
    }

    public bool IsEnglish => Language == "en";
    public bool IsAuthenticated => _http.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
