namespace Rafeeq.Domain.Common;

/// <summary>
/// Sends transactional email (OTP, notifications). v1 uses free SMTP; the Infrastructure impl logs
/// instead of sending when SMTP isn't configured, so local dev needs no credentials.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}