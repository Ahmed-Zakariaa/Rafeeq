using Rafeeq.Domain.Common;
using Rafeeq.Domain.Geography;

namespace Rafeeq.Domain.Identity;

public class User : BaseEntity<int>
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsEmailVerified { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public Gender Gender { get; private set; }
    public string? ProfilePhotoUrl { get; private set; }

    public int CountryId { get; private set; }
    public Country? Country { get; private set; }

    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactPhone { get; private set; }

    public AccountStatus AccountStatus { get; private set; } = AccountStatus.Active;
    public VerificationLevel VerificationLevel { get; private set; } = VerificationLevel.Unverified;

    // Ops bootstrap account — has every permission, hidden from lists/lookups. See admin-rbac direction.
    public bool IsSuperAdmin { get; private set; }
    // Self-registered users are activated immediately; admin-invited users activate by email first.
    public bool IsActivated { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    // Email verification OTP (free-path: emailed code instead of paid SMS).
    public string? EmailOtp { get; private set; }
    public DateTime? EmailOtpExpiry { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private User() { }

    /// <summary>Self-registration: password is set, account is active immediately.</summary>
    public User(string fullName, string email, string phoneNumber, string passwordHash, Gender gender, int countryId)
    {
        FullName = fullName;
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        Gender = gender;
        CountryId = countryId;
        IsActivated = true;
    }

    /// <summary>Admin-invited account: no password yet, must be activated by email before login.</summary>
    public static User CreateInvited(string fullName, string email, string phoneNumber, Gender gender, int countryId)
        => new()
        {
            FullName = fullName,
            Email = email.Trim().ToLowerInvariant(),
            PhoneNumber = phoneNumber,
            PasswordHash = string.Empty,
            Gender = gender,
            CountryId = countryId,
            IsActivated = false,
            AccountStatus = AccountStatus.Active,
        };

    public void SetPasswordResetToken(string token, DateTime expiryUtc)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiry = expiryUtc;
    }

    /// <summary>Used by both initial activation and password reset. Clicking the emailed link proves
    /// email access, so this also marks the email verified.</summary>
    public void CompletePasswordSet(string passwordHash)
    {
        PasswordHash = passwordHash;
        IsActivated = true;
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
        VerifyEmail();
    }

    public void MarkSuperAdmin() => IsSuperAdmin = true;

    public void SetEmailOtp(string code, DateTime expiryUtc)
    {
        EmailOtp = code;
        EmailOtpExpiry = expiryUtc;
    }

    /// <summary>Returns false if the code is wrong/expired; on success verifies the email and clears the OTP.</summary>
    public bool ConfirmEmail(string code)
    {
        if (EmailOtp is null || EmailOtpExpiry is null || EmailOtpExpiry < DateTime.UtcNow || EmailOtp != code)
            return false;
        VerifyEmail();
        EmailOtp = null;
        EmailOtpExpiry = null;
        return true;
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        if (VerificationLevel < VerificationLevel.EmailVerified)
            VerificationLevel = VerificationLevel.EmailVerified;
    }

    public void MarkDocumentVerified() => VerificationLevel = VerificationLevel.DocumentVerified;
    public void Suspend() => AccountStatus = AccountStatus.Suspended;
    public void Ban() => AccountStatus = AccountStatus.Banned;
    public void Reactivate() => AccountStatus = AccountStatus.Active;
    public void SetProfilePhoto(string url) => ProfilePhotoUrl = url;
    public void SetEmergencyContact(string? name, string? phone)
    {
        EmergencyContactName = name;
        EmergencyContactPhone = phone;
    }
}
