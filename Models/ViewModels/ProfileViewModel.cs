using System.Collections.Generic;

namespace Career_Guidance_Platform.Models.ViewModels;

public class ProfileViewModel
{
    public User User { get; set; } = null!;
    public List<TestResult> TestResults { get; set; } = new();
    public List<Resume> Resumes { get; set; } = new();
    public List<JobApplication> JobApplications { get; set; } = new();
}
