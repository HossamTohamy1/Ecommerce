using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Commands.ResetPassword;

namespace ECommerce.API.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ResetPasswordModel(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public void OnGet(string? email, string? token)
    {
        Input.Email = email ?? string.Empty;
        Input.Token = token ?? string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(new ResetPasswordCommand(new ResetPasswordRequest
        {
            Email = Input.Email,
            Token = Input.Token,
            NewPassword = Input.NewPassword,
            ConfirmNewPassword = Input.ConfirmNewPassword
        }));

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.Error;
            return Page();
        }

        TempData["SuccessMessage"] = _localizer["Auth.ResetPassword.Title"].Value;
        return RedirectToPage("/Account/Login");
    }
}

