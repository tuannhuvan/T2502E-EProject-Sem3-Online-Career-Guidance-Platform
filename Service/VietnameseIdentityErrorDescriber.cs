using Microsoft.AspNetCore.Identity;

namespace Career_Guidance_Platform.Service
{
    public class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() => 
            new() { Code = nameof(DefaultError), Description = "Đã xảy ra lỗi không xác định." };

        public override IdentityError ConcurrencyFailure() => 
            new() { Code = nameof(ConcurrencyFailure), Description = "Lỗi bất đồng bộ, dữ liệu đã bị thay đổi." };

        public override IdentityError PasswordMismatch() => 
            new() { Code = nameof(PasswordMismatch), Description = "Mật khẩu không chính xác." };

        public override IdentityError InvalidToken() => 
            new() { Code = nameof(InvalidToken), Description = "Mã xác thực không hợp lệ." };

        public override IdentityError LoginAlreadyAssociated() => 
            new() { Code = nameof(LoginAlreadyAssociated), Description = "Tài khoản này đã được liên kết." };

        public override IdentityError InvalidUserName(string? userName) => 
            new() { Code = nameof(InvalidUserName), Description = $"Tên tài khoản '{userName}' không hợp lệ." };

        public override IdentityError InvalidEmail(string? email) => 
            new() { Code = nameof(InvalidEmail), Description = $"Email '{email}' không hợp lệ." };

        public override IdentityError DuplicateUserName(string? userName) => 
            new() { Code = nameof(DuplicateUserName), Description = $"Tên tài khoản '{userName}' đã tồn tại." };

        public override IdentityError DuplicateEmail(string? email) => 
            new() { Code = nameof(DuplicateEmail), Description = $"Email '{email}' đã được đăng ký bởi tài khoản khác." };

        public override IdentityError InvalidRoleName(string? role) => 
            new() { Code = nameof(InvalidRoleName), Description = $"Vai trò '{role}' không hợp lệ." };

        public override IdentityError DuplicateRoleName(string? role) => 
            new() { Code = nameof(DuplicateRoleName), Description = $"Vai trò '{role}' đã tồn tại." };

        public override IdentityError UserAlreadyHasPassword() => 
            new() { Code = nameof(UserAlreadyHasPassword), Description = "Người dùng đã thiết lập mật khẩu." };

        public override IdentityError UserLockoutNotEnabled() => 
            new() { Code = nameof(UserLockoutNotEnabled), Description = "Tài khoản không hỗ trợ tự động khóa." };

        public override IdentityError UserAlreadyInRole(string? role) => 
            new() { Code = nameof(UserAlreadyInRole), Description = $"Người dùng đã có vai trò '{role}'." };

        public override IdentityError UserNotInRole(string? role) => 
            new() { Code = nameof(UserNotInRole), Description = $"Người dùng chưa có vai trò '{role}'." };

        public override IdentityError PasswordTooShort(int length) => 
            new() { Code = nameof(PasswordTooShort), Description = $"Mật khẩu phải dài tối thiểu {length} ký tự." };

        public override IdentityError PasswordRequiresNonAlphanumeric() => 
            new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Mật khẩu phải chứa ít nhất một ký tự đặc biệt (ví dụ: @, #, $, ...)." };

        public override IdentityError PasswordRequiresDigit() => 
            new() { Code = nameof(PasswordRequiresDigit), Description = "Mật khẩu phải chứa ít nhất một chữ số ('0'-'9')." };

        public override IdentityError PasswordRequiresLower() => 
            new() { Code = nameof(PasswordRequiresLower), Description = "Mật khẩu phải chứa ít nhất một chữ thường ('a'-'z')." };

        public override IdentityError PasswordRequiresUpper() => 
            new() { Code = nameof(PasswordRequiresUpper), Description = "Mật khẩu phải chứa ít nhất một chữ hoa ('A'-'Z')." };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => 
            new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Mật khẩu phải chứa ít nhất {uniqueChars} ký tự khác nhau." };
    }
}
