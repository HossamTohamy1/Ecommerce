namespace ECommerce.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetToken, CancellationToken ct = default);

    Task SendEmailConfirmationAsync(string toEmail, string fullName, Guid userId, string confirmationToken, CancellationToken ct = default);

    Task SendTwoFactorCodeAsync(string toEmail, string fullName, string code, CancellationToken ct = default);
}
