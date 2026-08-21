using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ECommerce.Application.Common.Validation;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> RequiredField<T>(
        this IRuleBuilder<T, string> rule, IStringLocalizer<SharedResource> localizer)
    {
        return rule.NotEmpty().WithMessage(localizer["Validation.Required"].Value);
    }

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(
        this IRuleBuilder<T, string> rule, IStringLocalizer<SharedResource> localizer)
    {
        return rule.EmailAddress().WithMessage(localizer["Validation.Email"].Value);
    }

    public static IRuleBuilderOptions<T, string> LengthBetween<T>(
        this IRuleBuilder<T, string> rule, int min, int max, IStringLocalizer<SharedResource> localizer)
    {
        return rule.Length(min, max).WithMessage(localizer["Validation.Length"].Value);
    }
}
