namespace ECommerce.Infrastructure.Email;

public enum EmailWorkItemType
{
    PasswordReset,
    EmailConfirmation,
    TwoFactorCode
}

public record EmailWorkItem(
    EmailWorkItemType Type,
    string ToEmail,
    string FullName,
    string? TokenOrCode = null,
    Guid? UserId = null);
