using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Addresses.Commands.CreateAddress;

public record CreateAddressCommand(string UserId, SaveAddressRequest Request) : IRequest<Result<AddressDto>>;
