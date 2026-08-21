using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<Result>;
