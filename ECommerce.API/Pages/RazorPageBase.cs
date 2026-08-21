using System.Security.Claims;

namespace ECommerce.API.Pages;

public abstract class RazorPageBase : PageModel
{
    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No authenticated user id on this request.");

    protected void SetSuccess(IStringLocalizer<SharedResource> localizer)
        => TempData["SuccessMessage"] = localizer["Common.OperationSuccessful"].Value;

    protected void SetError(string? message)
        => TempData["ErrorMessage"] = message;
}

