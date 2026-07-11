using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity.DTOs;

namespace Rafeeq.Domain.Identity.IService;

public interface IAuthService
{
    Task<ResultViewModel<AuthResultDto>> Register(RegisterDto dto);
    Task<ResultViewModel<AuthResultDto>> Login(LoginDto dto);
    Task<ResultViewModel<bool>> ForgotPassword(ForgotPasswordDto dto);
    Task<ResultViewModel<bool>> ResetPassword(ResetPasswordDto dto);
    Task<ResultViewModel<AuthResultDto>> VerifyEmail(VerifyEmailDto dto);   // returns a fresh token (emailVerified=true)
    Task<ResultViewModel<bool>> ResendOtp();                       // resend the email code
}

/// <summary>Hashes/verifies passwords (PBKDF2 impl in Infrastructure — no external package).</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public record TokenResult(string Token, DateTime ExpiresAt);

/// <summary>Issues signed JWTs with role + permission claims.</summary>
public interface IJwtTokenService
{
    TokenResult Generate(int userId, string fullName, IEnumerable<string> roles,
        IEnumerable<string> permissions, bool isSuperAdmin = false, bool emailVerified = false);
}
