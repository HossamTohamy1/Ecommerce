using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(ForgotPasswordRequest Request) : IRequest<Result>;
