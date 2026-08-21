using ECommerce.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;

namespace ECommerce.API.Pages.Account;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthDefaults.Scheme);
        return RedirectToPage("/Index");
    }

    public IActionResult OnGet()
    {
        return RedirectToPage("/Index");
    }
}
