namespace ECommerce.Infrastructure.Email;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "no-reply@ecommerce.local";
    public string FromName { get; set; } = "ECommerce";
    public bool EnableSsl { get; set; } = true;

    public string ResetPasswordUrl { get; set; } = "https://localhost/reset-password";

    public string ConfirmEmailUrl { get; set; } = "https://localhost/api/auth/confirm-email";
}
