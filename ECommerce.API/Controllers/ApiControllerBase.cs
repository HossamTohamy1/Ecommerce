using System.Security.Claims;

namespace ECommerce.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{

    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No authenticated user id on this request.");

    protected bool TryGetCurrentUserId(out Guid userId)
        => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
