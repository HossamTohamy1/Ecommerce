using System.Security.Claims;
using ECommerce.API.Security;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Commands.ExternalLogin;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.API.Pages.Account;

/// <summary>
/// Landing page for both Google and Facebook after they finish authenticating the user.
/// The provider's claims are picked up from the short-lived "Identity.External" cookie
/// (see DependencyInjection.AddInfrastructure) via SignInManager, never trusted from the URL.
/// </summary>
public class ExternalLoginCallbackModel : RazorPageBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<ExternalLoginCallbackModel> _logger;

    public ExternalLoginCallbackModel(
        SignInManager<ApplicationUser> signInManager,
        IMediator mediator,
        IStringLocalizer<SharedResource> localizer,
        ILogger<ExternalLoginCallbackModel> logger)
    {
        _signInManager = signInManager;
        _mediator = mediator;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError is not null)
        {
            TempData["ErrorMessage"] = _localizer["Auth.ExternalLogin.ProviderError", remoteError].Value;
            return RedirectToPage("/Account/Login");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            TempData["ErrorMessage"] = _localizer["Auth.ExternalLogin.Failed"].Value;
            return RedirectToPage("/Account/Login");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            // Facebook accounts without a verified/public email don't hand one over — nothing we can do.
            TempData["ErrorMessage"] = _localizer["Auth.ExternalLogin.NoEmail"].Value;
            return RedirectToPage("/Account/Login");
        }

        var fullName = info.Principal.FindFirstValue(ClaimTypes.Name);

        var result = await _mediator.Send(new ExternalLoginCommand(new ExternalLoginRequest
        {
            Provider = info.LoginProvider,
            ProviderKey = info.ProviderKey,
            Email = email,
            FullName = fullName
        }));

        // The external handshake is done either way — drop the temporary cookie now.
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        if (!result.Succeeded || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToPage("/Account/Login");
        }

        if (result.Data.RequiresTwoFactor)
        {
            // The linked account still has 2FA on — hand off to the normal Login page's OTP step.
            TempData["InfoMessage"] = _localizer["Auth.TwoFactor.CodeSent"].Value;
            return RedirectToPage("/Account/Login", new { challengeId = result.Data.TwoFactorChallengeId, returnUrl });
        }

        await CookieSignInHelper.SignInAsync(HttpContext, result.Data);

        if (!string.IsNullOrEmpty(returnUrl))
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            _logger.LogWarning("Rejected non-local returnUrl during external login callback: {ReturnUrl}", returnUrl);
        }

        return RedirectToPage("/Index");
    }
}

