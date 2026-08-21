namespace ECommerce.Application.Features.Addresses.Commands.DeleteAddress;

public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteAddressCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(DeleteAddressCommand command, CancellationToken ct)
    {
        var address = await _context.Set<Address>().FirstOrDefaultAsync(a => a.Id == command.Id && a.UserId == command.UserId, ct);
        if (address is null)
        {
            return Result.Failure(_localizer["Address.NotFound"].Value);
        }

        address.IsDeleted = true;
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
