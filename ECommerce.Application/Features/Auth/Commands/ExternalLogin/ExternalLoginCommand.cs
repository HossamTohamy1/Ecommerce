using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Features.Auth.Commands.ExternalLogin;

public record ExternalLoginCommand(ExternalLoginRequest Request) : IRequest<Result<AuthResponse>>;
