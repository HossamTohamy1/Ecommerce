
namespace ECommerce.API.Pages;

[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }

    public void OnGet()
    {
        RequestId = HttpContext.TraceIdentifier;
    }
}
