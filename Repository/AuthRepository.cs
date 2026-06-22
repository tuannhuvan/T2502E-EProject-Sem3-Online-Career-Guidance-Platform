using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Career_Guidance_Platform.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthRepository(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IdentityResult> RegisterAsync(
        User user,
        string password)
    {
        return await _userManager.CreateAsync(
            user,
            password);
    }

    public async Task AddRoleAsync(
        User user,
        string role)
    {
        await _userManager.AddToRoleAsync(
            user,
            role);
    }

    public async Task<User?> FindByEmailAsync(
        string email)
    {
        return await _userManager
            .FindByEmailAsync(email);
    }

    public async Task<SignInResult> LoginAsync(
        string email,
        string password,
        bool rememberMe)
    {
        return await _signInManager
            .PasswordSignInAsync(
                email,
                password,
                rememberMe,
                true);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<string>
        GeneratePasswordResetTokenAsync(
            User user)
    {
        return await _userManager
            .GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult>
        ResetPasswordAsync(
            User user,
            string token,
            string password)
    {
        return await _userManager
            .ResetPasswordAsync(
                user,
                token,
                password);
    }
}