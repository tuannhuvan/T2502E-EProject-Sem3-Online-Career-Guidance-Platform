using System.ComponentModel.DataAnnotations;

namespace Career_Guidance_Platform.Dtos.Auth;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}