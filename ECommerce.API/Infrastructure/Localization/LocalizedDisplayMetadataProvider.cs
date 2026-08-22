using System.ComponentModel.DataAnnotations;
using ECommerce.Shared.Resources;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;

namespace ECommerce.API.Infrastructure.Localization;

public class LocalizedDisplayMetadataProvider : IDisplayMetadataProvider
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LocalizedDisplayMetadataProvider(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        var propertyName = context.Key.Name;
        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        var displayAttr = context.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
        if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name) && displayAttr.ResourceType != null)
        {
            return;
        }

        context.DisplayMetadata.DisplayName = () =>
        {
            var localized = _localizer[propertyName];
            if (!localized.ResourceNotFound)
            {
                return localized.Value;
            }

            var prefixes = new[] { "Common.", "Product.", "Auth.", "Discount.", "Order.", "Catalog.Category.", "Address." };
            foreach (var prefix in prefixes)
            {
                var prefixedLoc = _localizer[prefix + propertyName];
                if (!prefixedLoc.ResourceNotFound)
                {
                    return prefixedLoc.Value;
                }
            }

            return propertyName;
        };
    }
}
