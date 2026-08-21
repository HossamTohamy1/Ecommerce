namespace ECommerce.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;

    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }

    public ICollection<Category> SubCategories { get; private set; } = new List<Category>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Category()
    {
    }

    public static Category Create(string name, string slug, Guid? parentCategoryId, string actorId)
    {
        var category = new Category { CreatedById = actorId };
        category.SetDetails(name, slug, parentCategoryId);
        return category;
    }

    public void UpdateDetails(string name, string slug, Guid? parentCategoryId, bool isActive, string actorId)
    {
        SetDetails(name, slug, parentCategoryId);
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = actorId;
    }

    private void SetDetails(string name, string slug, Guid? parentCategoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category.NameRequired", "A category must have a name.");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("Category.SlugRequired", "A category must have a slug.");
        }

        if (parentCategoryId.HasValue && parentCategoryId.Value == Id)
        {
            throw new DomainException("Category.CannotBeSelfParent", "A category cannot be its own parent.");
        }

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        ParentCategoryId = parentCategoryId;
    }
}
