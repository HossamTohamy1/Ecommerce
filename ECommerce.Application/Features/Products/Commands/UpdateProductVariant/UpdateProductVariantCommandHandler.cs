using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Products.Commands.UpdateProductVariant;

public class UpdateProductVariantCommandHandler : IRequestHandler<UpdateProductVariantCommand, Result<ProductVariantDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UpdateProductVariantCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<ProductVariantDto>> Handle(UpdateProductVariantCommand command, CancellationToken ct)
    {
        var product = await _context.Set<Product>()
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, ct);

        if (product is null)
        {
            return Result<ProductVariantDto>.Failure(_localizer["Catalog.Product.NotFound"].Value);
        }

        if (await _context.Set<ProductVariant>().AnyAsync(v => v.SKU == command.Request.SKU && v.Id != command.VariantId, ct))
        {
            return Result<ProductVariantDto>.Failure(_localizer["Catalog.Variant.DuplicateSku"].Value);
        }

        try
        {
            product.UpdateVariant(command.VariantId, command.Request.SKU, command.Request.Price, command.Request.StockQuantity, command.Request.Size, command.Request.Color, command.Request.IsActive, command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<ProductVariantDto>.Failure(LocalizeDomainError(ex));
        }

        var variant = product.Variants.First(v => v.Id == command.VariantId);

        await _context.SaveChangesAsync(ct);

        return Result<ProductVariantDto>.Success(new ProductVariantDto
        {
            Id = variant.Id,
            SKU = variant.SKU,
            Price = variant.Price,
            StockQuantity = variant.StockQuantity,
            Size = variant.Size,
            Color = variant.Color
        });
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Variant.NotFound" => _localizer["Catalog.Variant.NotFound"].Value,
        _ => ex.Message
    };
}
