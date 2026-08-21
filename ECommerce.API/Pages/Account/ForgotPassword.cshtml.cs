using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Commands.ForgotPassword;

namespace ECommerce.API.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly IMediator _mediator;

    public ForgotPasswordModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool EmailSent { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _mediator.Send(new ForgotPasswordCommand(new ForgotPasswordRequest { Email = Input.Email }));

        EmailSent = true;
        return Page();
    }
}

