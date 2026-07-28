using System.ComponentModel.DataAnnotations;

namespace Career_Guidance_Platform.Dtos.Auth;

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; }
}