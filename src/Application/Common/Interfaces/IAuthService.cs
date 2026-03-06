using Application.Common.Models.User;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<UserSignInResponse> SignIn(UserSignInRequest request);
    Task<UserSignUpResponse> SignUp(UserSignUpRequest request);
    Task Logout(string? refreshToken);
    Task<RefreshTokenResponse> RefreshToken(string refreshToken);
    Task<UserProfileResponse> GetProfile();
    Task<UserProfileResponse> UpdateProfile(UpdateProfileRequest request);
}
