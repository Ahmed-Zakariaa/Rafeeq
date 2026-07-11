namespace Rafeeq.Domain.Common;

/// <summary>
/// Abstraction over the authenticated request (implemented in Infrastructure via IHttpContextAccessor),
/// so Application/Domain stay free of ASP.NET types.
/// </summary>
public interface ICurrentUser
{
    int? UserId { get; }
    string Language { get; }          // "ar" (default) | "en"
    bool IsEnglish { get; }
    bool IsAuthenticated { get; }
}
