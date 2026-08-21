using System.Security.Claims;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Commands.Register;
using ECommerce.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;

namespace ECommerce.API.Pages.Account;

public class RegisterModel : RazorPageBase
{
    private readonly IMediator _mediator;

    public RegisterModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var request = new RegisterRequest
        {
            FullName = Input.FullName,
            Email = Input.Email,
            Password = Input.Password,
            ConfirmPassword = Input.ConfirmPassword
        };

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(new RegisterCommand(request));


        if (!result.Succeeded || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Error;
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Data.UserId.ToString()),
            new(ClaimTypes.Email, result.Data.Email),
            new(ClaimTypes.Name, result.Data.FullName)
        };
        claims.AddRange(result.Data.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, CookieAuthDefaults.Scheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthDefaults.Scheme, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        });

        return RedirectToPage("/Index");
    }
}
