using Career_Guidance_Platform.Dtos.Auth;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Repository.Interfaces;
using Career_Guidance_Platform.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Career_Guidance_Platform.Service;

public class AuthService : IAuthService 
{
     private readonly IAuthRepository _authRepository;
    private readonly IEmailSender _emailSender;

    public AuthService(
        IAuthRepository authRepository,
        IEmailSender emailSender)
    {
        _authRepository = authRepository;
        _emailSender = emailSender;
    }

    public async Task<IdentityResult>
        RegisterAsync(
            RegisterViewModel model)
    {
        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            UserName = model.Email
        };

        var result =
            await _authRepository.RegisterAsync(
                user,
                model.Password);

        if (result.Succeeded)
        {
            await _authRepository.AddRoleAsync(
                user,
                "User");
        }

        return result;
    }

    public async Task<SignInResult>
        LoginAsync(
            LoginViewModel model)
    {
        return await _authRepository.LoginAsync(
            model.Email,
            model.Password,
            model.RememberMe);
    }

    public async Task LogoutAsync()
    {
        await _authRepository.LogoutAsync();
    }

    public async Task<bool>
        SendResetPasswordEmailAsync(
            string email,
            string callbackUrl)
    {
        var user =
            await _authRepository
                .FindByEmailAsync(email);

        if (user == null)
            return false;

        var token =
            await _authRepository
                .GeneratePasswordResetTokenAsync(user);

        var link =
            $"{callbackUrl}?email={email}&token={Uri.EscapeDataString(token)}";

        await _emailSender.SendEmailAsync(
            email,
            "Reset Password",
            $"<a href='{link}'>Đặt lại mật khẩu</a>");

        return true;
    }

    public async Task<IdentityResult>
        ResetPasswordAsync(
            ResetPasswordViewModel model)
    {
        var user =
            await _authRepository
                .FindByEmailAsync(model.Email);

        if (user == null)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "User not found"
                });
        }

        return await _authRepository
            .ResetPasswordAsync(
                user,
                model.Token,
                model.Password);
    }
}