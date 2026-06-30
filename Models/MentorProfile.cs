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

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public ICollection<MentorshipRequest> MentorshipRequests { get; set; } = new List<MentorshipRequest>();
        public ICollection<MentorshipMeeting> MentorshipMeetings { get; set; } = new List<MentorshipMeeting>();
        public ICollection<GroupMentoringSession> GroupMentoringSessions { get; set; } = new List<GroupMentoringSession>();
    }
}
