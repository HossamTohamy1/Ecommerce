using System.ComponentModel.DataAnnotations;
using System.Globalization;
using ECommerce.Shared.Resources;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace ECommerce.API.Infrastructure.Localization;

public class CustomValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
{
    private readonly ValidationAttributeAdapterProvider _inner = new();
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CustomValidationAttributeAdapterProvider(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public IAttributeAdapter? GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer)
    {
        var loc = stringLocalizer ?? _localizer;

        return attribute switch
        {
            RequiredAttribute req => new LocalizedRequiredAttributeAdapter(req, loc),
            StringLengthAttribute strLen => new LocalizedStringLengthAttributeAdapter(strLen, loc),
            RangeAttribute range => new LocalizedRangeAttributeAdapter(range, loc),
            EmailAddressAttribute email => new LocalizedEmailAddressAttributeAdapter(email, loc),
            RegularExpressionAttribute regex => new LocalizedRegularExpressionAttributeAdapter(regex, loc),
            CompareAttribute comp => new LocalizedCompareAttributeAdapter(comp, loc),
            _ => _inner.GetAttributeAdapter(attribute, loc)
        };
    }
}

public class LocalizedRequiredAttributeAdapter : AttributeAdapterBase<RequiredAttribute>
{
    private readonly IStringLocalizer _localizer;

    public LocalizedRequiredAttributeAdapter(RequiredAttribute attribute, IStringLocalizer stringLocalizer)
        : base(attribute, stringLocalizer)
    {
        _localizer = stringLocalizer;
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-required", GetErrorMessage(context));
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        var displayName = validationContext.ModelMetadata.GetDisplayName();
        if (!string.IsNullOrEmpty(Attribute.ErrorMessage))
        {
            return _localizer[Attribute.ErrorMessage, displayName].Value;
        }

        return _localizer["The {0} field is required.", displayName].Value;
    }
}

public class LocalizedStringLengthAttributeAdapter : AttributeAdapterBase<StringLengthAttribute>
{
    private readonly IStringLocalizer _localizer;

    public LocalizedStringLengthAttributeAdapter(StringLengthAttribute attribute, IStringLocalizer stringLocalizer)
        : base(attribute, stringLocalizer)
    {
        _localizer = stringLocalizer;
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-length", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-length-max", Attribute.MaximumLength.ToString(CultureInfo.InvariantCulture));
        if (Attribute.MinimumLength > 0)
        {
            MergeAttribute(context.Attributes, "data-val-length-min", Attribute.MinimumLength.ToString(CultureInfo.InvariantCulture));
        }
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        var displayName = validationContext.ModelMetadata.GetDisplayName();
        if (!string.IsNullOrEmpty(Attribute.ErrorMessage))
        {
            return _localizer[Attribute.ErrorMessage, displayName, Attribute.MaximumLength, Attribute.MinimumLength].Value;
        }

        if (Attribute.MinimumLength > 0)
        {
            return _localizer["The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.", displayName, Attribute.MaximumLength, Attribute.MinimumLength].Value;
        }

        return _localizer["The field {0} must be a string with a maximum length of {1}.", displayName, Attribute.MaximumLength].Value;
    }
}

public class LocalizedRangeAttributeAdapter : AttributeAdapterBase<RangeAttribute>
{
    private readonly IStringLocalizer _localizer;

    public LocalizedRangeAttributeAdapter(RangeAttribute attribute, IStringLocalizer stringLocalizer)
        : base(attribute, stringLocalizer)
    {
        _localizer = stringLocalizer;
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-range", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-range-min", Convert.ToString(Attribute.Minimum, CultureInfo.InvariantCulture)!);
        MergeAttribute(context.Attributes, "data-val-range-max", Convert.ToString(Attribute.Maximum, CultureInfo.InvariantCulture)!);
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        var displayName = validationContext.ModelMetadata.GetDisplayName();
        if (!string.IsNullOrEmpty(Attribute.ErrorMessage))
        {
            return _localizer[Attribute.ErrorMessage, displayName, Attribute.Minimum, Attribute.Maximum].Value;
        }

        return _localizer["The field {0} must be between {1} and {2}.", displayName, Attribute.Minimum, Attribute.Maximum].Value;
    }
}

public class LocalizedEmailAddressAttributeAdapter : AttributeAdapterBase<EmailAddressAttribute>
{
    private readonly IStringLocalizer _localizer;

    public LocalizedEmailAddressAttributeAdapter(EmailAddressAttribute attribute, IStringLocalizer stringLocalizer)
        : base(attribute, stringLocalizer)
    {
        _localizer = stringLocalizer;
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-email", GetErrorMessage(context));
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        var displayName = validationContext.ModelMetadata.GetDisplayName();
        if (!string.IsNullOrEmpty(Attribute.ErrorMessage))
        {
            return _localizer[Attribute.ErrorMessage, displayName].Value;
        }

        return _localizer["The {0} field is not a valid e-mail address.", displayName].Value;
    }
}

public class LocalizedRegularExpressionAttributeAdapter : AttributeAdapterBase<RegularExpressionAttribute>
{
    private readonly IStringLocalizer _localizer;

    public LocalizedRegularExpressionAttributeAdapter(RegularExpressionAttribute attribute, IStringLocalizer stringLocalizer)
        : base(attribute, stringLocalizer)
    {
        _localizer = stringLocalizer;
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-regex", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-regex-pattern", Attribute.Pattern);
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        var displayName = validationContext.ModelMetadata.GetDisplayName();
        if (!string.IsNullOrEmpty(Attribute.ErrorMessage))
        {
            return _localizer[Attribute.ErrorMessage, displayName, Attribute.Pattern].Value;
        }

        return _localizer["The field {0} must match the regular expression '{1}'.", displayName, Attribute.Pattern].Value;
    }
}

public class LocalizedCompareAttributeAdapter : AttributeAdapterBase<CompareAttribute>
{
    private readonly IStringLocalizer _localizer;

    public LocalizedCompareAttributeAdapter(CompareAttribute attribute, IStringLocalizer stringLocalizer)
        : base(attribute, stringLocalizer)
    {
        _localizer = stringLocalizer;
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-equalto", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-equalto-other", "*." + Attribute.OtherProperty);
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        var displayName = validationContext.ModelMetadata.GetDisplayName();
        var otherDisplayName = Attribute.OtherPropertyDisplayName ?? Attribute.OtherProperty;
        if (!string.IsNullOrEmpty(Attribute.ErrorMessage))
        {
            return _localizer[Attribute.ErrorMessage, displayName, otherDisplayName].Value;
        }

        return _localizer["'{0}' and '{1}' do not match.", displayName, otherDisplayName].Value;
    }
}
