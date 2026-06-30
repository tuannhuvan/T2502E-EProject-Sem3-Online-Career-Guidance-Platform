using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("job_applications")]
    public class JobApplication
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("job_posting_id")]
        public int JobPostingId { get; set; }

        [Column("resume_id")]
        public int? ResumeId { get; set; }

        [Required]
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Applied"; // Applied, Interviewing, Offered, Rejected, Withdrawn

        [Column("applied_at")]
        public DateTime AppliedAt { get; set; } = DateTime.Now;

        [Column("notes")]
        public string? Notes { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("JobPostingId")]
        public JobPosting? JobPosting { get; set; }

        [ForeignKey("ResumeId")]
        public Resume? Resume { get; set; }

        public ICollection<ApplicationReminder> Reminders { get; set; } = new List<ApplicationReminder>();
    }
}
