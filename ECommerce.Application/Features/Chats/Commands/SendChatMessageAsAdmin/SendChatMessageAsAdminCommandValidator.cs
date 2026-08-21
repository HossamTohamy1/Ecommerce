using FluentValidation;

namespace ECommerce.Application.Features.Chats.Commands.SendChatMessageAsAdmin;

public class SendChatMessageAsAdminCommandValidator : AbstractValidator<SendChatMessageAsAdminCommand>
{
    public SendChatMessageAsAdminCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.ConversationId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.AdminId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Content).NotEmpty().WithMessage(localizer["Validation.Required"].Value)
                .MaximumLength(2000).WithMessage(localizer["Validation.MaxLength"].Value);
        });
    }
}
