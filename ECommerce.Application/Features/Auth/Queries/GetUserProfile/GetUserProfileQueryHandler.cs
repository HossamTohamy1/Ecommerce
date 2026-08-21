using ECommerce.Application.DTOs.Auth;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Auth.Queries.GetUserProfile;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GetUserProfileQueryHandler(
        UserManager<ApplicationUser> userManager,
        IStringLocalizer<SharedResource> localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }

    public async Task<Result<UserProfileResponse>> Handle(GetUserProfileQuery request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return Result<UserProfileResponse>.Failure(_localizer["Auth.User.NotFound"]);
        }

        var roles = await _userManager.GetRolesAsync(user);

        var profile = user.Adapt<UserProfileResponse>();
        profile.FullName = user.FullName;
        profile.Roles = roles;
        profile.CreatedAtUtc = user.CreatedAtUtc;
        return Result<UserProfileResponse>.Success(profile);
    }
}
