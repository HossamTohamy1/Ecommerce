namespace ECommerce.API.Pages.Chat.Admin;

public class IndexModel : RazorPageBase
{
    [BindProperty(SupportsGet = true)]
    public Guid? ConversationId { get; set; }

    public void OnGet()
    {
    }
}
