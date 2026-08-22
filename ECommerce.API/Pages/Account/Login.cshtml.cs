using ECommerce.API.Security;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Commands.Login;
using ECommerce.Application.Features.Auth.Commands.VerifyTwoFactor;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Configuration;

namespace ECommerce.API.Pages.Account;

public class LoginModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly IConfiguration _configuration;

    public LoginModel(IMediator mediator, IStringLocalizer<SharedResource> localizer, IConfiguration configuration)
    {
        _mediator = mediator;
        _localizer = localizer;
        _configuration = configuration;
    }


    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public bool AwaitingTwoFactorCode => Input.TwoFactorChallengeId.HasValue;

    public bool IsGoogleEnabled => HasConfigValue("Authentication:Google:ClientId") && HasConfigValue("Authentication:Google:ClientSecret");
    public bool IsFacebookEnabled => HasConfigValue("Authentication:Facebook:AppId") && HasConfigValue("Authentication:Facebook:AppSecret");

    public class InputModel
    {
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public Guid? TwoFactorChallengeId { get; set; }

        public string? Code { get; set; }
    }

    public void OnGet(string? returnUrl = null, Guid? challengeId = null)
    {
        ReturnUrl = returnUrl;

        if (challengeId.HasValue)
        {
            Input.TwoFactorChallengeId = challengeId;
        }
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (Input.TwoFactorChallengeId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(Input.Code))
            {
                TempData["ErrorMessage"] = _localizer["Auth.TwoFactor.InvalidCode"].Value;
                return Page();
            }

            var verifyResult = await _mediator.Send(new VerifyTwoFactorCommand(new VerifyTwoFactorRequest
            {
                ChallengeId = Input.TwoFactorChallengeId.Value,
                Code = Input.Code
            }));

            if (!verifyResult.Succeeded || verifyResult.Data is null)
            {
                TempData["ErrorMessage"] = verifyResult.Error;
                return Page();
            }

            await CookieSignInHelper.SignInAsync(HttpContext, verifyResult.Data);
            return RedirectAfterLogin(returnUrl);
        }

        var request = new LoginRequest
        {
            Email = Input.Email,
            Password = Input.Password
        };

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(new LoginCommand(request));

        if (!result.Succeeded || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Error;
            return Page();
        }

        if (result.Data.RequiresTwoFactor)
        {
            Input.TwoFactorChallengeId = result.Data.TwoFactorChallengeId;
            Input.Password = string.Empty;
            TempData["InfoMessage"] = _localizer["Auth.TwoFactor.CodeSent"].Value;
            return Page();
        }

        await CookieSignInHelper.SignInAsync(HttpContext, result.Data);
        return RedirectAfterLogin(returnUrl);
    }

    public IActionResult OnPostGoogle(string? returnUrl = null)
    {
        if (!IsGoogleEnabled)
        {
            TempData["ErrorMessage"] = _localizer["Auth.ExternalLogin.NotConfigured"].Value;
            return RedirectToPage(new { returnUrl });
        }

        var redirectUrl = Url.Page("/Account/ExternalLoginCallback", pageHandler: null, values: new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    public IActionResult OnPostFacebook(string? returnUrl = null)
    {
        if (!IsFacebookEnabled)
        {
            TempData["ErrorMessage"] = _localizer["Auth.ExternalLogin.NotConfigured"].Value;
            return RedirectToPage(new { returnUrl });
        }

        var redirectUrl = Url.Page("/Account/ExternalLoginCallback", pageHandler: null, values: new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, FacebookDefaults.AuthenticationScheme);
    }

    private IActionResult RedirectAfterLogin(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage("/Index");
    }

    private bool HasConfigValue(string key) => !string.IsNullOrWhiteSpace(_configuration[key]);
}
