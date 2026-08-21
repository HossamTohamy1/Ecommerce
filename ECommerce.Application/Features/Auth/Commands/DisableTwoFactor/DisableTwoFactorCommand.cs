using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Features.Auth.Commands.DisableTwoFactor;

public record DisableTwoFactorCommand(Guid UserId, EnableTwoFactorRequest Request) : IRequest<Result>;
