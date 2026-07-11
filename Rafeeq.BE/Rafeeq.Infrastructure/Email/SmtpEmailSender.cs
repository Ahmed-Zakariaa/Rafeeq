using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Rafeeq.Domain.Common;

namespace Rafeeq.Infrastructure.Email;

/// <summary>
/// Sends email over SMTP when configured (the "Email" section); otherwise logs the message so local
/// dev without credentials still works. Free-path: any SMTP (e.g. Gmail with an app password).
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _s;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(EmailSettings settings, ILogger<SmtpEmailSender> logger)
    {
        _s = settings;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_s.Host))
        {
            _logger.LogWarning("EMAIL not configured → {To} | {Subject}\n{Body}", toEmail, subject, htmlBody);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(
                string.IsNullOrWhiteSpace(_s.FromEmail) ? _s.User : _s.FromEmail, _s.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_s.Host, _s.Port)
        {
            EnableSsl = _s.EnableSsl,
            Credentials = new NetworkCredential(_s.User, _s.Password),
        };

        try
        {
            await client.SendMailAsync(message, ct);
            _logger.LogInformation("EMAIL sent → {To} | {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            // Don't fail the user's action because email delivery failed.
            _logger.LogError(ex, "EMAIL send failed → {To} | {Subject}", toEmail, subject);
        }
    }
}
