using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Identity;

namespace Career_Guidance_Platform.Repository.Interfaces;

public interface IAuthRepository
{
    Task<IdentityResult> RegisterAsync(
        User user,
        string password);

    Task AddRoleAsync(
        User user,
        string role);

    Task<User?> FindByEmailAsync(
        string email);

    Task<SignInResult> LoginAsync(
        string email,
        string password,
        bool rememberMe);

    Task LogoutAsync();

    Task<string> GeneratePasswordResetTokenAsync(
        User user);

    Task<IdentityResult> ResetPasswordAsync(
        User user,
        string token,
        string password);
}