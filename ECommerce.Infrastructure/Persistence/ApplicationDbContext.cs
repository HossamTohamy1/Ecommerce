using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ECommerce.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private static readonly HashSet<string> ExcludedFromAudit = new()
    {
        nameof(AuditLog),
        nameof(Notification),
        nameof(ChatMessage),
        nameof(ChatConversation)
    };

    // Note: RefreshToken / EmailConfirmationToken / TwoFactorCode don't derive from BaseEntity
    // (same as PasswordResetToken), so they are never picked up by BuildAuditEntries() below —
    // no explicit exclusion needed, but documented here to avoid confusion.

    private readonly ICurrentUserService? _currentUserService;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null,
        IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<ProductDiscount> ProductDiscounts => Set<ProductDiscount>();

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailConfirmationToken> EmailConfirmationTokens => Set<EmailConfirmationToken>();
    public DbSet<TwoFactorCode> TwoFactorCodes => Set<TwoFactorCode>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ChatConversation> ChatConversations => Set<ChatConversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = BuildAuditEntries();
        foreach (var entry in auditEntries)
        {
            Set<AuditLog>().Add(entry);
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private List<AuditLog> BuildAuditEntries()
    {
        var result = new List<AuditLog>();

        var userId = _currentUserService?.UserId?.ToString();
        var userName = _httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        var ipAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            var entityName = entry.Entity.GetType().Name;
            if (ExcludedFromAudit.Contains(entityName))
            {
                continue;
            }

            AuditAction? action = entry.State switch
            {
                EntityState.Added => AuditAction.Create,
                EntityState.Deleted => AuditAction.Delete,
                EntityState.Modified => IsSoftDelete(entry) ? AuditAction.Delete : AuditAction.Update,
                _ => null
            };

            if (action is null)
            {
                continue;
            }

            var changes = action switch
            {
                AuditAction.Create => BuildCreateJson(entry),
                AuditAction.Update => BuildChangesJson(entry),
                AuditAction.Delete => BuildDeleteJson(entry),
                _ => null
            };

            if (action == AuditAction.Update && changes is null)
            {
                continue;
            }

            result.Add(new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action.Value,
                EntityName = entityName,
                EntityId = entry.Entity.Id.ToString(),
                Changes = changes,
                IpAddress = ipAddress
            });
        }

        return result;
    }

    private static bool IsSoftDelete(EntityEntry<BaseEntity> entry)
    {
        var isDeletedProperty = entry.Property(e => e.IsDeleted);
        return isDeletedProperty.IsModified && !(bool)isDeletedProperty.OriginalValue! && (bool)isDeletedProperty.CurrentValue!;
    }

    private static string? BuildCreateJson(EntityEntry<BaseEntity> entry)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.Name is nameof(BaseEntity.CreatedAt) or nameof(BaseEntity.CreatedById)
                or nameof(BaseEntity.UpdatedAt) or nameof(BaseEntity.UpdatedById) or nameof(BaseEntity.IsDeleted))
            {
                continue;
            }

            if (property.CurrentValue is not null)
            {
                values[property.Metadata.Name] = property.CurrentValue;
            }
        }

        EnrichDisplayNames(entry, values);

        return values.Count == 0 ? null : JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = false });
    }

    private static string? BuildDeleteJson(EntityEntry<BaseEntity> entry)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.Name is nameof(BaseEntity.CreatedAt) or nameof(BaseEntity.CreatedById)
                or nameof(BaseEntity.UpdatedAt) or nameof(BaseEntity.UpdatedById) or nameof(BaseEntity.IsDeleted))
            {
                continue;
            }

            var val = property.CurrentValue ?? property.OriginalValue;
            if (val is not null)
            {
                values[property.Metadata.Name] = val;
            }
        }

        EnrichDisplayNames(entry, values);

        return values.Count == 0 ? null : JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = false });
    }

    private static void EnrichDisplayNames(EntityEntry<BaseEntity> entry, Dictionary<string, object?> values)
    {
        switch (entry.Entity)
        {
            case CartItem ci:
                if (!string.IsNullOrEmpty(ci.Product?.Name)) values["ProductName"] = ci.Product.Name;
                if (ci.ProductVariant != null) values["VariantLabel"] = $"{ci.ProductVariant.Size} {ci.ProductVariant.Color}".Trim();
                break;
            case OrderItem oi:
                if (!string.IsNullOrEmpty(oi.Product?.Name)) values["ProductName"] = oi.Product.Name;
                break;
            case Product p:
                if (!string.IsNullOrEmpty(p.Category?.Name)) values["CategoryName"] = p.Category.Name;
                if (!string.IsNullOrEmpty(p.Brand?.Name)) values["BrandName"] = p.Brand.Name;
                break;
            case ProductVariant pv:
                if (!string.IsNullOrEmpty(pv.Product?.Name)) values["ProductName"] = pv.Product.Name;
                break;
            case ProductReview pr:
                if (!string.IsNullOrEmpty(pr.Product?.Name)) values["ProductName"] = pr.Product.Name;
                break;
            case ProductImage pi:
                if (!string.IsNullOrEmpty(pi.Product?.Name)) values["ProductName"] = pi.Product.Name;
                break;
            case Category c:
                if (!string.IsNullOrEmpty(c.ParentCategory?.Name)) values["ParentCategoryName"] = c.ParentCategory.Name;
                break;
            case Order o:
                if (o.OrderNumber is not null) values["OrderNumber"] = o.OrderNumber.Value;
                break;
            case WishlistItem wi:
                if (!string.IsNullOrEmpty(wi.Product?.Name)) values["ProductName"] = wi.Product.Name;
                break;
            case ProductDiscount pd:
                if (!string.IsNullOrEmpty(pd.Product?.Name)) values["ProductName"] = pd.Product.Name;
                if (!string.IsNullOrEmpty(pd.Discount?.Name)) values["DiscountName"] = pd.Discount.Name;
                break;
            case Address a:
                if (!string.IsNullOrEmpty(a.FullName)) values["RecipientName"] = a.FullName;
                break;
            case Discount d:
                if (!string.IsNullOrEmpty(d.Name)) values["DiscountName"] = d.Name;
                break;
            case Notification n:
                if (!string.IsNullOrEmpty(n.Title)) values["NotificationTitle"] = n.Title;
                break;
        }
    }

    private static string? BuildChangesJson(EntityEntry<BaseEntity> entry)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (!property.IsModified)
            {
                continue;
            }

            if (property.Metadata.Name is nameof(BaseEntity.UpdatedAt) or nameof(BaseEntity.UpdatedById))
            {
                continue;
            }

            if (Equals(property.OriginalValue, property.CurrentValue))
            {
                continue;
            }

            changes[property.Metadata.Name] = new
            {
                old = property.OriginalValue,
                @new = property.CurrentValue
            };
        }

        if (changes.Count == 0) return null;

        var companion = new Dictionary<string, object?>();
        EnrichDisplayNames(entry, companion);
        foreach (var kvp in companion)
        {
            changes[kvp.Key] = kvp.Value;
        }

        return JsonSerializer.Serialize(changes, new JsonSerializerOptions { WriteIndented = false });
    }
}
