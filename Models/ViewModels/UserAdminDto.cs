namespace Career_Guidance_Platform.Models.ViewModels
{
    public class UserAdminDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Student";
        public string Status { get; set; } = "Active";
        public string? Password { get; set; }
    }
}
