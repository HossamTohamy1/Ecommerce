using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Features.Auth.Commands.Login;

public record LoginCommand(LoginRequest Request) : IRequest<Result<AuthResponse>>;
