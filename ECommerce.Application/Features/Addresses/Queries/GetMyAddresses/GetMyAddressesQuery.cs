using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Addresses.Queries.GetMyAddresses;

public record GetMyAddressesQuery(string UserId) : IRequest<List<AddressDto>>;
