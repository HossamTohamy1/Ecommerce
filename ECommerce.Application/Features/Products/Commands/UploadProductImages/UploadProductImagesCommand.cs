using ECommerce.Application.DTOs.Catalog;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Features.Products.Commands.UploadProductImages;

public record UploadProductImagesCommand(Guid ProductId, List<IFormFile> Images, string UserId) : IRequest<Result<List<ProductImageDto>>>;
