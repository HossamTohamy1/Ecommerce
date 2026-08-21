using System.Net;
using System.Net.Mail;

namespace ECommerce.Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, IStringLocalizer<SharedResource> localizer, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetToken, CancellationToken ct = default)
    {
        var resetLink = $"{_settings.ResetPasswordUrl}?email={Uri.EscapeDataString(toEmail)}&token={Uri.EscapeDataString(resetToken)}";

        var subject = _localizer["Email.PasswordReset.Subject"].Value;
        var body = _localizer["Email.PasswordReset.Body", fullName, resetLink].Value;

        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning(_localizer["Email.SmtpNotConfigured", toEmail, resetLink].Value);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string fullName, Guid userId, string confirmationToken, CancellationToken ct = default)
    {
        var confirmLink = $"{_settings.ConfirmEmailUrl}?userId={userId}&token={Uri.EscapeDataString(confirmationToken)}";

        var subject = _localizer["Email.Confirmation.Subject"].Value;
        var body = _localizer["Email.Confirmation.Body", fullName, confirmLink].Value;

        await SendAsync(toEmail, subject, body, confirmLink, ct);
    }

    public async Task SendTwoFactorCodeAsync(string toEmail, string fullName, string code, CancellationToken ct = default)
    {
        var subject = _localizer["Email.TwoFactor.Subject"].Value;
        var body = _localizer["Email.TwoFactor.Body", fullName, code].Value;

        await SendAsync(toEmail, subject, body, code, ct);
    }

    private async Task SendAsync(string toEmail, string subject, string body, string fallbackLogValue, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning(_localizer["Email.SmtpNotConfigured", toEmail, fallbackLogValue].Value);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("Email '{Subject}' sent to {Email}", subject, toEmail);
    }
}
