using System.Threading.Channels;
using Microsoft.Extensions.Hosting;

namespace ECommerce.Infrastructure.Email;

public class EmailBackgroundWorker : BackgroundService
{
    private readonly Channel<EmailWorkItem> _channel;
    private readonly SmtpEmailService _smtpService;
    private readonly ILogger<EmailBackgroundWorker> _logger;

    public EmailBackgroundWorker(
        Channel<EmailWorkItem> channel,
        SmtpEmailService smtpService,
        ILogger<EmailBackgroundWorker> logger)
    {
        _channel = channel;
        _smtpService = smtpService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailBackgroundWorker started.");

        while (await _channel.Reader.WaitToReadAsync(stoppingToken))
        {
            while (_channel.Reader.TryRead(out var item))
            {
                try
                {
                    switch (item.Type)
                    {
                        case EmailWorkItemType.PasswordReset:
                            await _smtpService.SendPasswordResetEmailAsync(item.ToEmail, item.FullName, item.TokenOrCode!, stoppingToken);
                            break;

                        case EmailWorkItemType.EmailConfirmation:
                            await _smtpService.SendEmailConfirmationAsync(item.ToEmail, item.FullName, item.UserId!.Value, item.TokenOrCode!, stoppingToken);
                            break;

                        case EmailWorkItemType.TwoFactorCode:
                            await _smtpService.SendTwoFactorCodeAsync(item.ToEmail, item.FullName, item.TokenOrCode!, stoppingToken);
                            break;
                    }
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Failed to send background email of type {Type} to {Email}", item.Type, item.ToEmail);
                }
            }
        }

        _logger.LogInformation("EmailBackgroundWorker stopping.");
    }
}
