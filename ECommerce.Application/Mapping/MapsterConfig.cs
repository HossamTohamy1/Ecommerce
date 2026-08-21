using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.DTOs.Shopping;
using Mapster;

namespace ECommerce.Application.Mapping;


public static class MapsterConfig
{
    public static void RegisterMappings()
    {

        TypeAdapterConfig<ApplicationUser, AuthResponse>.NewConfig()
            .Map(dest => dest.UserId, src => src.Id)
            .Map(dest => dest.Email, src => src.Email)
            .IgnoreNonMapped(true);

        TypeAdapterConfig<ApplicationUser, UserProfileResponse>.NewConfig()
            .Map(dest => dest.UserId, src => src.Id)
            .Map(dest => dest.Email, src => src.Email)
            .IgnoreNonMapped(true);

        TypeAdapterConfig<Category, CategoryDto>.NewConfig()
            .Map(dest => dest.ParentCategoryName, src => src.ParentCategory != null ? src.ParentCategory.Name : null)
            .Map(dest => dest.ProductCount, src => src.Products.Count(p => !p.IsDeleted));

        TypeAdapterConfig<Brand, BrandDto>.NewConfig()
            .Map(dest => dest.ProductCount, src => src.Products.Count(p => !p.IsDeleted));


        TypeAdapterConfig<Address, AddressDto>.NewConfig();
    }
}
