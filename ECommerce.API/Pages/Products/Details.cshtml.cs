using ECommerce.Application.DTOs.Reviews;
using ECommerce.Application.Features.Carts.Commands.AddToCart;
using ECommerce.Application.Features.ProductReviews.Commands.CreateProductReview;
using ECommerce.Application.Features.ProductReviews.Queries.GetApprovedReviewsForProduct;
using ECommerce.Application.Features.Products.Queries.GetProductById;
using ECommerce.Application.Features.Wishlists.Commands.AddToWishlist;

namespace ECommerce.API.Pages.Products;

public class DetailsModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DetailsModel(
        IMediator mediator,
        IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ProductDto? Product { get; set; }
    public List<ProductReviewDto> Reviews { get; set; } = new();

    [BindProperty]
    public ReviewInputModel ReviewInput { get; set; } = new();

    public class ReviewInputModel
    {
        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [StringLength(2000)]
        public string? Comment { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _mediator.Send(new GetProductByIdQuery(Id));
        if (!result.Succeeded)
        {
            return NotFound();
        }

        Product = result.Data;
        Reviews = await _mediator.Send(new GetApprovedReviewsForProductQuery(Id));
        return Page();
    }


    private bool RequireLogin(out IActionResult? redirect)
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            redirect = null;
            return true;
        }

        redirect = RedirectToPage("/Account/Login", new { returnUrl = $"/Products/Details/{Id}" });
        return false;
    }

    public async Task<IActionResult> OnPostAddToCartAsync(Guid? variantId, int quantity = 1)
    {
        if (!RequireLogin(out var redirect)) return redirect!;

        var result = await _mediator.Send(new AddToCartCommand(CurrentUserId, new AddCartItemRequest
        {
            ProductId = Id,
            ProductVariantId = variantId,
            Quantity = quantity
        }));

        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage(new { id = Id });
    }


    public async Task<IActionResult> OnPostAddToWishlistAsync()
    {
        if (!RequireLogin(out var redirect)) return redirect!;

        var result = await _mediator.Send(new AddToWishlistCommand(CurrentUserId, Id));
        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostSubmitReviewAsync()
    {
        if (!RequireLogin(out var redirect)) return redirect!;

        var result = await _mediator.Send(new CreateProductReviewCommand(CurrentUserId, new CreateProductReviewRequest
        {
            ProductId = Id,
            Rating = ReviewInput.Rating,
            Comment = ReviewInput.Comment
        }));

        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage(new { id = Id });
    }
}

