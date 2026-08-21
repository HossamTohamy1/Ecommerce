using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(LogoutRequest Request) : IRequest<Result>;
