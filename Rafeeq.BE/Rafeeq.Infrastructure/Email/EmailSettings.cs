namespace Rafeeq.Infrastructure.Email;

/// <summary>SMTP config (bound from the "Email" section). Leave Host empty to fall back to logging.</summary>
public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;     // STARTTLS on 587
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Rafeeq";
}
