namespace ECommerce.Domain.Entities;

public class ProductReview : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public string UserId { get; private set; } = string.Empty;

    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public bool IsApproved { get; private set; }

    private ProductReview()
    {
    }

    public static ProductReview Create(Guid productId, string userId, int rating, string? comment, string actorId)
    {
        ValidateRating(rating);

        return new ProductReview
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Comment = comment?.Trim(),
            IsApproved = false,
            CreatedById = actorId
        };
    }

    public void UpdateOwn(int rating, string? comment, string actorId)
    {
        ValidateRating(rating);

        Rating = rating;
        Comment = comment?.Trim();
        IsApproved = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = actorId;
    }

    public void Approve() => IsApproved = true;

    private static void ValidateRating(int rating)
    {
        if (rating is < 1 or > 5)
        {
            throw new DomainException("Review.InvalidRating", "Rating must be between 1 and 5.");
        }
    }
}
