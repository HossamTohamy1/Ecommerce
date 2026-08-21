using ECommerce.Application.DTOs.Shopping;
using Mapster;

namespace ECommerce.Application.Features.Addresses.Queries.GetMyAddresses;

public class GetMyAddressesQueryHandler : IRequestHandler<GetMyAddressesQuery, List<AddressDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMyAddressesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AddressDto>> Handle(GetMyAddressesQuery request, CancellationToken ct)
    {
        return await _context.Set<Address>()
            .AsNoTracking()
            .Where(a => a.UserId == request.UserId)
            .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt)
            .ProjectToType<AddressDto>()
            .ToListAsync(ct);
    }
}
