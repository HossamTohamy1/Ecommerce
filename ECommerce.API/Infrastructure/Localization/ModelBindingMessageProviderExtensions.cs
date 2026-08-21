using ECommerce.Shared.Resources;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;

namespace ECommerce.API.Infrastructure.Localization;

public static class ModelBindingMessageProviderExtensions
{
    public static void ConfigureLocalizedModelBindingMessages(this DefaultModelBindingMessageProvider provider, IStringLocalizer<SharedResource> localizer)
    {
        provider.SetAttemptedValueIsInvalidAccessor((value, name) =>
            localizer["Validation.ModelBinding.AttemptedValueIsInvalid", value, name].Value);

        provider.SetMissingBindRequiredValueAccessor(name =>
            localizer["Validation.ModelBinding.MissingBindRequiredValue", name].Value);

        provider.SetMissingKeyOrValueAccessor(() =>
            localizer["Validation.ModelBinding.MissingKeyOrValue"].Value);

        provider.SetMissingRequestBodyRequiredValueAccessor(() =>
            localizer["Validation.ModelBinding.MissingRequestBodyRequiredValue"].Value);

        provider.SetNonPropertyAttemptedValueIsInvalidAccessor(value =>
            localizer["Validation.ModelBinding.NonPropertyAttemptedValueIsInvalid", value].Value);

        provider.SetNonPropertyUnknownValueIsInvalidAccessor(() =>
            localizer["Validation.ModelBinding.NonPropertyUnknownValueIsInvalid"].Value);

        provider.SetNonPropertyValueMustBeANumberAccessor(() =>
            localizer["Validation.ModelBinding.NonPropertyValueMustBeANumber"].Value);

        provider.SetUnknownValueIsInvalidAccessor(name =>
            localizer["Validation.ModelBinding.UnknownValueIsInvalid", name].Value);

        provider.SetValueIsInvalidAccessor(value =>
            localizer["Validation.ModelBinding.ValueIsInvalid", value].Value);

        provider.SetValueMustBeANumberAccessor(name =>
            localizer["The field {0} must be a number.", name].Value);

        provider.SetValueMustNotBeNullAccessor(name =>
            localizer["The {0} field is required.", name].Value);
    }
}
