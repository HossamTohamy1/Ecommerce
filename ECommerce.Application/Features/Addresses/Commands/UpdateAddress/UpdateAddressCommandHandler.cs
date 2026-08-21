using ECommerce.Application.DTOs.Shopping;
using Mapster;

namespace ECommerce.Application.Features.Addresses.Commands.UpdateAddress;

public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, Result<AddressDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UpdateAddressCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<AddressDto>> Handle(UpdateAddressCommand command, CancellationToken ct)
    {
        var address = await _context.Set<Address>().FirstOrDefaultAsync(a => a.Id == command.Id && a.UserId == command.UserId, ct);
        if (address is null)
        {
            return Result<AddressDto>.Failure(_localizer["Address.NotFound"].Value);
        }

        if (command.Request.IsDefault && !address.IsDefault)
        {
            var currentDefaults = await _context.Set<Address>()
                .Where(a => a.UserId == command.UserId && a.IsDefault)
                .ToListAsync(ct);

            foreach (var a in currentDefaults)
            {
                a.UnsetDefault();
            }
        }

        try
        {
            address.UpdateDetails(
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

        await _context.SaveChangesAsync(ct);

        return Result<AddressDto>.Success(address.Adapt<AddressDto>());
    }
}
