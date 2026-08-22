using System.Threading.Channels;
using ECommerce.Application.Interfaces;

namespace ECommerce.Infrastructure.Email;

public class QueuedEmailService : IEmailService
{
    private readonly Channel<EmailWorkItem> _channel;
    private readonly ILogger<QueuedEmailService> _logger;

    public QueuedEmailService(Channel<EmailWorkItem> channel, ILogger<QueuedEmailService> logger)
    {
        _channel = channel;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetToken, CancellationToken ct = default)
    {
        var item = new EmailWorkItem(EmailWorkItemType.PasswordReset, toEmail, fullName, TokenOrCode: resetToken);
        await _channel.Writer.WriteAsync(item, ct);
        _logger.LogDebug("Enqueued password reset email for {Email}", toEmail);
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string fullName, Guid userId, string confirmationToken, CancellationToken ct = default)
    {
        var item = new EmailWorkItem(EmailWorkItemType.EmailConfirmation, toEmail, fullName, TokenOrCode: confirmationToken, UserId: userId);
        await _channel.Writer.WriteAsync(item, ct);
        _logger.LogDebug("Enqueued email confirmation for {Email}", toEmail);
    }

    public async Task SendTwoFactorCodeAsync(string toEmail, string fullName, string code, CancellationToken ct = default)
    {
        var item = new EmailWorkItem(EmailWorkItemType.TwoFactorCode, toEmail, fullName, TokenOrCode: code);
        await _channel.Writer.WriteAsync(item, ct);
        _logger.LogDebug("Enqueued 2FA code email for {Email}", toEmail);
    }
}
