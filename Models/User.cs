using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Career_Guidance_Platform.Models;

[Table("users")]
public class User : IdentityUser<int>
{
    [Required]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Column("role")]
    public string Role { get; set; } = "Student";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("created_by")]
    public string CreatedBy { get; set; } = "System";

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("updated_by")]
    public string? UpdatedBy { get; set; }

    [Column("status")]
    public int Status { get; set; } = 1;

    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public MentorProfile? MentorProfile { get; set; }
    public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    public ICollection<EmployerReview> EmployerReviews { get; set; } = new List<EmployerReview>();
    public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
    public ICollection<CommunityPost> CommunityPosts { get; set; } = new List<CommunityPost>();
    public ICollection<CommunityComment> CommunityComments { get; set; } = new List<CommunityComment>();
    public ICollection<MentorshipRequest> MenteeRequests { get; set; } = new List<MentorshipRequest>();
    public ICollection<MentorshipRequest> MentorRequests { get; set; } = new List<MentorshipRequest>();
    public ICollection<MentorshipMeeting> MenteeMeetings { get; set; } = new List<MentorshipMeeting>();
    public ICollection<MentorshipMeeting> MentorMeetings { get; set; } = new List<MentorshipMeeting>();
    public ICollection<MentorshipMessage> SentMessages { get; set; } = new List<MentorshipMessage>();
    public ICollection<MentorshipMessage> ReceivedMessages { get; set; } = new List<MentorshipMessage>();
    public ICollection<GroupMentoringRegistration> GroupMentoringRegistrations { get; set; } = new List<GroupMentoringRegistration>();
    public ICollection<PeerConnection> PeerConnectionsSent { get; set; } = new List<PeerConnection>();
    public ICollection<PeerConnection> PeerConnectionsReceived { get; set; } = new List<PeerConnection>();
}