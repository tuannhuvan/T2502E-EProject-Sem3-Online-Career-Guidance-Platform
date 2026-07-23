using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("group_mentoring_sessions")]
    public class GroupMentoringSession
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("mentor_id")]
        public int MentorId { get; set; }

        [Required]
        [Column("title")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("scheduled_time")]
        public DateTime ScheduledTime { get; set; }

        [Column("meeting_url")]
        [StringLength(255)]
        public string? MeetingUrl { get; set; }

        [Column("max_participants")]
        public int MaxParticipants { get; set; } = 20;

        [Required]
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("MentorId")]
        public User? Mentor { get; set; }

        public ICollection<GroupMentoringRegistration> Registrations { get; set; } = new List<GroupMentoringRegistration>();
    }
}
