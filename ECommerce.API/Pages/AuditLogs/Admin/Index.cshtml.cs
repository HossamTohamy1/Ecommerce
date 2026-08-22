using ECommerce.Application.DTOs.Audit;
using ECommerce.Application.Features.AuditLogs.Queries.GetPagedAuditLogs;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Pages.AuditLogs.Admin;

public class IndexModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IServiceScopeFactory _scopeFactory;

    public IndexModel(IMediator mediator, IServiceScopeFactory scopeFactory)
    {
        _mediator = mediator;
        _scopeFactory = scopeFactory;
    }

    [BindProperty(SupportsGet = true, Name = "page")]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    [BindProperty(SupportsGet = true)]
    public string? EntityName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? UserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public AuditAction? Action { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromUtc { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToUtc { get; set; }

    public PagedResult<AuditLogDto> Result { get; set; } = new();
    public Dictionary<string, string> EntityLookup { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public List<string> EntityNames { get; } = new()
    {
        "Product", "Category", "Brand", "ProductVariant", "Discount", "ProductDiscount",
        "Order", "OrderItem", "OrderStatusHistory", "Address", "ProductReview",
        "Wishlist", "WishlistItem", "Cart", "CartItem", "Notification", "ChatConversation"
    };

    public async Task OnGetAsync([FromQuery] int? page, [FromQuery] int? pageNumber, [FromQuery] int? pageSize)
    {
        var p = (page.HasValue && page.Value > 0) ? page.Value :
                ((pageNumber.HasValue && pageNumber.Value > 0) ? pageNumber.Value :
                (PageNumber > 0 ? PageNumber : 1));

        var ps = pageSize.HasValue && pageSize.Value > 0 ? pageSize.Value :
                 (PageSize is >= 1 and <= 200 ? PageSize : 10);

        PageNumber = p;
        PageSize = ps;

        var filter = new AuditLogFilter
        {
            EntityName = EntityName,
            UserId = UserId,
            Action = Action,
            FromUtc = FromUtc,
            ToUtc = ToUtc
        };

        Result = await _mediator.Send(new GetPagedAuditLogsQuery(filter, p, ps));
        await PopulateEntityLookupAsync();
    }

    private async Task PopulateEntityLookupAsync()
    {
        if (Result.Items.Count == 0) return;

        var guidRegex = new System.Text.RegularExpressions.Regex(
            @"\b[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}\b",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        var parsedGuids = new HashSet<Guid>();

        foreach (var item in Result.Items)
        {
            if (Guid.TryParse(item.EntityId, out var eid)) parsedGuids.Add(eid);
            if (Guid.TryParse(item.UserId, out var uid)) parsedGuids.Add(uid);

            if (!string.IsNullOrEmpty(item.Changes))
            {
                foreach (System.Text.RegularExpressions.Match match in guidRegex.Matches(item.Changes))
                {
                    if (Guid.TryParse(match.Value, out var gid))
                    {
                        parsedGuids.Add(gid);
                    }
                }
            }
        }

        if (parsedGuids.Count == 0) return;

        var guidList = parsedGuids.ToList();

        var usersTask = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await db.Users.AsNoTracking()
                .Where(u => guidList.Contains(u.Id))
                .Select(u => new { Id = u.Id.ToString(), Name = !string.IsNullOrWhiteSpace(u.FullName) ? u.FullName : u.Email })
                .ToListAsync();
        });

        var productsTask = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await db.Products.AsNoTracking()
                .Where(p => guidList.Contains(p.Id))
                .Select(p => new { Id = p.Id.ToString(), p.Name })
                .ToListAsync();
        });

        var categoriesTask = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await db.Categories.AsNoTracking()
                .Where(c => guidList.Contains(c.Id))
                .Select(c => new { Id = c.Id.ToString(), c.Name })
                .ToListAsync();
        });

        var brandsTask = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await db.Brands.AsNoTracking()
                .Where(b => guidList.Contains(b.Id))
                .Select(b => new { Id = b.Id.ToString(), b.Name })
                .ToListAsync();
        });

        var wishlistsTask = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await db.Wishlists.AsNoTracking()
                .Where(w => guidList.Contains(w.Id))
                .Select(w => new { Id = w.Id.ToString(), w.UserId })
                .ToListAsync();
        });

        var addressesTask = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await db.Addresses.AsNoTracking()
                .Where(a => guidList.Contains(a.Id))
                .Select(a => new { Id = a.Id.ToString(), a.FullName, a.City })
                .ToListAsync();
        });

        var ordersTask = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await db.Orders.AsNoTracking()
                .Where(o => guidList.Contains(o.Id))
                .Select(o => new { Id = o.Id.ToString(), OrderNumber = o.OrderNumber.Value })
                .ToListAsync();
        });

        var discountsTask = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await db.Discounts.AsNoTracking()
                .Where(d => guidList.Contains(d.Id))
                .Select(d => new { Id = d.Id.ToString(), d.Name, d.Code })
                .ToListAsync();
        });

        var variantsTask = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await db.ProductVariants.AsNoTracking()
                .Where(v => guidList.Contains(v.Id))
                .Select(v => new { Id = v.Id.ToString(), ProductName = v.Product.Name, v.Size, v.Color })
                .ToListAsync();
        });

        await Task.WhenAll(usersTask, productsTask, categoriesTask, brandsTask, wishlistsTask, addressesTask, ordersTask, discountsTask, variantsTask);

        foreach (var u in await usersTask)
        {
            if (!string.IsNullOrWhiteSpace(u.Name)) EntityLookup[u.Id] = u.Name;
        }

        foreach (var p in await productsTask)
        {
            if (!string.IsNullOrWhiteSpace(p.Name)) EntityLookup[p.Id] = p.Name;
        }

        foreach (var c in await categoriesTask)
        {
            if (!string.IsNullOrWhiteSpace(c.Name)) EntityLookup[c.Id] = c.Name;
        }

        foreach (var b in await brandsTask)
        {
            if (!string.IsNullOrWhiteSpace(b.Name)) EntityLookup[b.Id] = b.Name;
        }

        var wishlists = await wishlistsTask;
        var missingWishlistUserGuids = wishlists
            .Where(w => !string.IsNullOrEmpty(w.UserId) && !EntityLookup.ContainsKey(w.UserId) && Guid.TryParse(w.UserId, out _))
            .Select(w => Guid.Parse(w.UserId))
            .Distinct()
            .ToList();

        if (missingWishlistUserGuids.Count > 0)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var moreUsers = await db.Users.AsNoTracking()
                .Where(u => missingWishlistUserGuids.Contains(u.Id))
                .Select(u => new { Id = u.Id.ToString(), Name = !string.IsNullOrWhiteSpace(u.FullName) ? u.FullName : u.Email })
                .ToListAsync();

            foreach (var u in moreUsers)
            {
                if (!string.IsNullOrWhiteSpace(u.Name)) EntityLookup[u.Id] = u.Name;
            }
        }

        foreach (var w in wishlists)
        {
            if (!string.IsNullOrEmpty(w.UserId) && EntityLookup.TryGetValue(w.UserId, out var ownerName))
            {
                EntityLookup[w.Id] = $"Wishlist of {ownerName}";
            }
            else
            {
                EntityLookup[w.Id] = "Customer Wishlist";
            }
        }

        foreach (var a in await addressesTask)
        {
            EntityLookup[a.Id] = !string.IsNullOrWhiteSpace(a.City) ? $"{a.FullName} ({a.City})" : a.FullName;
        }

        foreach (var o in await ordersTask)
        {
            if (!string.IsNullOrWhiteSpace(o.OrderNumber)) EntityLookup[o.Id] = o.OrderNumber;
        }

        foreach (var d in await discountsTask)
        {
            EntityLookup[d.Id] = !string.IsNullOrWhiteSpace(d.Code) ? $"{d.Name} ({d.Code})" : d.Name;
        }

        foreach (var v in await variantsTask)
        {
            var label = $"{v.Size} {v.Color}".Trim();
            EntityLookup[v.Id] = !string.IsNullOrWhiteSpace(v.ProductName)
                ? $"{v.ProductName} ({label})"
                : label;
        }
    }

    public class FormattedChangeItem
    {
        public string Label { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? SecondaryValue { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public bool IsDiff { get; set; }
    }

    public static List<FormattedChangeItem> ParseChanges(string? json, Dictionary<string, string>? lookup = null)
    {
        var list = new List<FormattedChangeItem>();
        if (string.IsNullOrWhiteSpace(json)) return list;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != System.Text.Json.JsonValueKind.Object) return list;

            var companionNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var valStr = prop.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(valStr))
                    {
                        if (prop.Name.EndsWith("Name", StringComparison.OrdinalIgnoreCase) ||
                            prop.Name.EndsWith("Label", StringComparison.OrdinalIgnoreCase) ||
                            prop.Name.EndsWith("Title", StringComparison.OrdinalIgnoreCase))
                        {
                            companionNames[prop.Name] = valStr;
                        }
                    }
                }
            }

            foreach (var prop in root.EnumerateObject())
            {
                var key = prop.Name;
                var val = prop.Value;

                if (key.EndsWith("Name", StringComparison.OrdinalIgnoreCase) || 
                    key.EndsWith("Label", StringComparison.OrdinalIgnoreCase) ||
                    key.EndsWith("Title", StringComparison.OrdinalIgnoreCase))
                {
                    var idKey = key.EndsWith("Name", StringComparison.OrdinalIgnoreCase) 
                        ? key[..^4] + "Id" 
                        : (key.EndsWith("Label", StringComparison.OrdinalIgnoreCase) ? key[..^5] + "Id" : key[..^5] + "Id");
                    
                    if (root.TryGetProperty(idKey, out _))
                    {
                        continue;
                    }
                }

                if (val.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    val.TryGetProperty("old", out var oldProp) &&
                    val.TryGetProperty("new", out var newProp))
                {
                    var oldRaw = FormatJsonElement(oldProp);
                    var newRaw = FormatJsonElement(newProp);

                    var (oldDisplay, _) = ResolveValueDisplay(key, oldRaw, companionNames, lookup);
                    var (newDisplay, _) = ResolveValueDisplay(key, newRaw, companionNames, lookup);

                    list.Add(new FormattedChangeItem
                    {
                        Label = FormatLabel(key),
                        OldValue = oldDisplay,
                        NewValue = newDisplay,
                        IsDiff = true
                    });
                }
                else
                {
                    string label = FormatLabel(key);
                    var rawVal = FormatJsonElement(val);
                    var (primary, secondary) = ResolveValueDisplay(key, rawVal, companionNames, lookup);

                    list.Add(new FormattedChangeItem
                    {
                        Label = label,
                        Value = primary,
                        SecondaryValue = secondary,
                        IsDiff = false
                    });
                }
            }
        }
        catch
        {
            list.Add(new FormattedChangeItem { Label = "Raw", Value = json, IsDiff = false });
        }

        return list;
    }

    private static (string Primary, string? Secondary) ResolveValueDisplay(
        string key,
        string rawVal,
        Dictionary<string, string> companionNames,
        Dictionary<string, string>? lookup)
    {
        if (string.IsNullOrWhiteSpace(rawVal) || rawVal == "—")
        {
            return (rawVal, null);
        }

        if (lookup != null && lookup.TryGetValue(rawVal, out var dbResolvedName) && !string.IsNullOrWhiteSpace(dbResolvedName))
        {
            var snippet = rawVal.Length > 8 ? rawVal[..8] : rawVal;
            return (dbResolvedName, $"#{snippet}");
        }

        if (key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && key.Length > 2)
        {
            var prefix = key[..^2];
            string? companion = null;
            if (companionNames.TryGetValue(prefix + "Name", out companion) ||
                companionNames.TryGetValue(prefix + "Label", out companion) ||
                companionNames.TryGetValue(prefix + "Title", out companion) ||
                (prefix.Equals("User", StringComparison.OrdinalIgnoreCase) && companionNames.TryGetValue("UserName", out companion)) ||
                (prefix.Equals("ShippingAddress", StringComparison.OrdinalIgnoreCase) && companionNames.TryGetValue("RecipientName", out companion)))
            {
                if (!string.IsNullOrWhiteSpace(companion))
                {
                    var snippet = rawVal.Length > 8 ? rawVal[..8] : rawVal;
                    return (companion, $"#{snippet}");
                }
            }
        }

        if (rawVal.Length == 36 && Guid.TryParse(rawVal, out _))
        {
            return ($"#{rawVal[..8]}", rawVal);
        }

        return (rawVal, null);
    }

    private static string FormatLabel(string key)
    {
        return key switch
        {
            "ProductName" => "Product",
            "CategoryName" => "Category",
            "BrandName" => "Brand",
            "ParentCategoryName" => "Parent Category",
            "VariantLabel" => "Variant",
            "StockQuantity" => "Stock",
            "CompareAtPrice" => "Compare Price",
            "UnitPrice" => "Unit Price",
            "IsActive" => "Active",
            "OrderNumber" => "Order #",
            "PaymentMethod" => "Payment Method",
            "PaymentStatus" => "Payment Status",
            "ShippingAddressId" => "Shipping Address",
            "UserId" => "User",
            "ProductId" => "Product",
            "CategoryId" => "Category",
            "BrandId" => "Brand",
            "ParentCategoryId" => "Parent Category",
            "ProductVariantId" => "Variant",
            "DiscountId" => "Discount",
            "WishlistId" => "Wishlist",
            "NotificationId" => "Notification",
            "AddressId" => "Address",
            "OrderId" => "Order",
            _ => System.Text.RegularExpressions.Regex.Replace(key, "([A-Z])", " $1").Trim()
        };
    }

    private static string FormatJsonElement(System.Text.Json.JsonElement el)
    {
        return el.ValueKind switch
        {
            System.Text.Json.JsonValueKind.Null => "—",
            System.Text.Json.JsonValueKind.True => "Yes",
            System.Text.Json.JsonValueKind.False => "No",
            System.Text.Json.JsonValueKind.String => el.GetString() ?? "—",
            System.Text.Json.JsonValueKind.Number => el.GetRawText(),
            _ => el.GetRawText()
        };
    }
}
