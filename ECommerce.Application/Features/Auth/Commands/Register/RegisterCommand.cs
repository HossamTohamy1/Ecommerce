using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterRequest Request) : IRequest<Result<AuthResponse>>;
