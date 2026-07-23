using Career_Guidance_Platform.Dtos.Auth;
using Microsoft.AspNetCore.Identity;

namespace Career_Guidance_Platform.Service.Interfaces;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(
        RegisterViewModel model);

    Task<SignInResult> LoginAsync(
        LoginViewModel model);

    Task LogoutAsync();

    Task<bool> SendResetPasswordEmailAsync(
        string email,
        string callbackUrl);

    Task<IdentityResult> ResetPasswordAsync(
        ResetPasswordViewModel model);
}