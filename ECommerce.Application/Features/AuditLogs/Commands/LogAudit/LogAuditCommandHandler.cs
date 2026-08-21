namespace ECommerce.Application.Features.AuditLogs.Commands.LogAudit;

public class LogAuditCommandHandler : IRequestHandler<LogAuditCommand>
{
    private readonly IApplicationDbContext _context;

    public LogAuditCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(LogAuditCommand command, CancellationToken ct)
    {
        _context.Set<AuditLog>().Add(new AuditLog
        {
            UserId = command.UserId,
            UserName = command.UserName,
            Action = command.Action,
            EntityName = command.EntityName,
            EntityId = command.EntityId,
            Description = command.Description,
            IpAddress = command.IpAddress
        });

        await _context.SaveChangesAsync(ct);
    }
}
