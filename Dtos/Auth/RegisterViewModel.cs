using System.ComponentModel.DataAnnotations;

namespace Career_Guidance_Platform.Dtos.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập Email")]
    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
    public string? Email { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập ten")]
    public string? FullName { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    public string? Password { get; set; }
   
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; }
}