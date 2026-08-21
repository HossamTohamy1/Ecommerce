using ECommerce.Application.DTOs.Shopping;
using Mapster;

namespace ECommerce.Application.Features.Addresses.Commands.CreateAddress;

public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, Result<AddressDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateAddressCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AddressDto>> Handle(CreateAddressCommand command, CancellationToken ct)
    {
        Address address;
        try
        {
            address = Address.Create(
                command.UserId,
                command.Request.FullName,
                command.Request.Phone,
                command.Request.Street,
                command.Request.City,
                command.Request.Governorate,
                command.Request.PostalCode,
                command.Request.IsDefault,
                command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<AddressDto>.Failure(ex.Message);
        }

        if (command.Request.IsDefault)
        {
            var currentDefaults = await _context.Set<Address>()
                .Where(a => a.UserId == command.UserId && a.IsDefault)
                .ToListAsync(ct);

            foreach (var a in currentDefaults)
            {
                a.UnsetDefault();
            }
        }

        _context.Set<Address>().Add(address);
        await _context.SaveChangesAsync(ct);

        return Result<AddressDto>.Success(address.Adapt<AddressDto>());
    }
}
