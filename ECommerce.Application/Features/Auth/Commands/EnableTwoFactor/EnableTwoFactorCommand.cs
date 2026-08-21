using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Features.Auth.Commands.EnableTwoFactor;

public record EnableTwoFactorCommand(Guid UserId, EnableTwoFactorRequest Request) : IRequest<Result>;
