using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Features.Auth.Queries.GetUserProfile;

public record GetUserProfileQuery(Guid UserId) : IRequest<Result<UserProfileResponse>>;
