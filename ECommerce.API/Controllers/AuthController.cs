using System.Security.Claims;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Commands.ConfirmEmail;
using ECommerce.Application.Features.Auth.Commands.DisableTwoFactor;
using ECommerce.Application.Features.Auth.Commands.EnableTwoFactor;
using ECommerce.Application.Features.Auth.Commands.ForgotPassword;
using ECommerce.Application.Features.Auth.Commands.Login;
using ECommerce.Application.Features.Auth.Commands.Logout;
using ECommerce.Application.Features.Auth.Commands.RefreshToken;
using ECommerce.Application.Features.Auth.Commands.Register;
using ECommerce.Application.Features.Auth.Commands.ResendConfirmationEmail;
using ECommerce.Application.Features.Auth.Commands.ResetPassword;
using ECommerce.Application.Features.Auth.Commands.VerifyTwoFactor;
using ECommerce.Application.Features.Auth.Queries.GetUserProfile;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AuthController(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterCommand(request), ct);
        if (!result.Succeeded)
        {
            return Conflict(new { message = result.Error });
        }

        return CreatedAtAction(nameof(Me), null, result.Data);
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token, CancellationToken ct)
    {
        var result = await _mediator.Send(new ConfirmEmailCommand(new ConfirmEmailRequest { UserId = userId, Token = token }), ct);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new
        {
            message = result.Data!.AlreadyConfirmed
                ? _localizer["Auth.ConfirmEmail.AlreadyConfirmed"].Value
                : _localizer["Auth.ConfirmEmail.Success"].Value
        });
    }

    [HttpPost("resend-confirmation")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendConfirmation([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ResendConfirmationEmailCommand(request.Email), ct);
        return Ok(new { message = _localizer["Auth.ForgotPassword.GenericMessage"].Value });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginCommand(request), ct);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPost("2fa/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyTwoFactorCommand(request), ct);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new EnableTwoFactorCommand(CurrentUserId, request), ct);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new { message = "Two-factor authentication enabled." });
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor([FromBody] EnableTwoFactorRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new DisableTwoFactorCommand(CurrentUserId, request), ct);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new { message = "Two-factor authentication disabled." });
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request), ct);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LogoutCommand(request), ct);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Error });
        }

        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ForgotPasswordCommand(request), ct);
        return Ok(new { message = _localizer["Auth.ForgotPassword.GenericMessage"].Value });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ResetPasswordCommand(request), ct);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new { message = _localizer["Auth.ResetPassword.Success"].Value });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserProfileQuery(CurrentUserId), ct);
        if (!result.Succeeded)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(result.Data);
    }

    private Guid CurrentUserId
    {
        get
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var id) ? id : throw new UnauthorizedAccessException("Missing or invalid user id claim.");
        }
    }
}

