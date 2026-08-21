using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Features.Auth.Commands.VerifyTwoFactor;

public record VerifyTwoFactorCommand(VerifyTwoFactorRequest Request) : IRequest<Result<AuthResponse>>;
