namespace ECommerce.Application.Features.Addresses.Commands.DeleteAddress;

public record DeleteAddressCommand(string UserId, Guid Id) : IRequest<Result>;
