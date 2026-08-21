using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Addresses.Commands.UpdateAddress;

public record UpdateAddressCommand(string UserId, Guid Id, SaveAddressRequest Request) : IRequest<Result<AddressDto>>;
