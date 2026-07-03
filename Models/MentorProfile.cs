using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("mentor_profiles")]
    public class MentorProfile
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("job_title")]
        [StringLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [Column("company")]
        [StringLength(100)]
        public string Company { get; set; } = string.Empty;

        [Required]
        [Column("specialization")]
        [StringLength(200)]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        [Column("biography")]
        public string Biography { get; set; } = string.Empty;

        [Column("availability_json", TypeName = "longtext")]
        public string? AvailabilityJson { get; set; }

        [Column("linkedin_url")]
        [StringLength(255)]
        public string? LinkedInUrl { get; set; }

        [Column("rating", TypeName = "decimal(3,2)")]
        public decimal Rating { get; set; } = 5.00m;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_verified")]
        public bool IsVerified { get; set; } = false;

        [Column("hourly_rate", TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; } = 0.00m;

        [Column("experience_description", TypeName = "longtext")]
        public string ExperienceDescription { get; set; } = string.Empty;

        [Column("expertise", TypeName = "longtext")]
        public string Expertise { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public ICollection<MentorshipRequest> MentorshipRequests { get; set; } = new List<MentorshipRequest>();
        public ICollection<MentorshipMeeting> MentorshipMeetings { get; set; } = new List<MentorshipMeeting>();
        public ICollection<GroupMentoringSession> GroupMentoringSessions { get; set; } = new List<GroupMentoringSession>();
    }
}
