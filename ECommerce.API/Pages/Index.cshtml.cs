
namespace ECommerce.API.Pages;

public class IndexModel : PageModel
{
    public bool IsAdmin => User.IsInRole(AppConstants.Roles.Admin);

    public void OnGet()
    {
    }
}
