using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(ConfirmEmailRequest Request) : IRequest<Result<ConfirmEmailResponse>>;
