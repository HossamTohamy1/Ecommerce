using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Shared.Resources;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ECommerce.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteProductCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _fileStorage = fileStorage;
        _localizer = localizer;
    }

    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken ct)
    {
        var product = await _context.Set<Product>()
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (product is null)
        {
            return Result.Failure(_localizer["Catalog.Product.NotFound"].Value);
        }

        var hasOrderHistory = await _context.Set<OrderItem>()
            .IgnoreQueryFilters()
            .AnyAsync(oi => oi.ProductId == command.Id, ct);

        if (!hasOrderHistory)
        {
            foreach (var image in product.Images)
            {
                if (!string.IsNullOrEmpty(image.ImageUrl) && !image.ImageUrl.StartsWith("/images/seed/"))
                {
                    await _fileStorage.DeleteAsync(image.ImageUrl, ct);
                }
            }

            var discounts = await _context.Set<ProductDiscount>()
                .Where(pd => pd.ProductId == command.Id)
                .ToListAsync(ct);
            _context.Set<ProductDiscount>().RemoveRange(discounts);

            var reviews = await _context.Set<ProductReview>()
                .Where(pr => pr.ProductId == command.Id)
                .ToListAsync(ct);
            foreach (var r in reviews)
            {
                r.IsDeleted = true;
            }
        }

        var cartItems = await _context.Set<CartItem>()
            .Where(ci => ci.ProductId == command.Id)
            .ToListAsync(ct);
        _context.Set<CartItem>().RemoveRange(cartItems);

        var wishlistItems = await _context.Set<WishlistItem>()
            .Where(wi => wi.ProductId == command.Id)
            .ToListAsync(ct);
        _context.Set<WishlistItem>().RemoveRange(wishlistItems);

        product.IsDeleted = true;
        product.IsActive = false;

        foreach (var variant in product.Variants)
        {
            variant.IsDeleted = true;
            variant.IsActive = false;
        }

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
