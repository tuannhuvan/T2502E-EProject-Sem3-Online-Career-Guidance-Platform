using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("saved_jobs")]
    public class SavedJob
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("job_posting_id")]
        public int JobPostingId { get; set; }

        [Column("saved_at")]
        public DateTime SavedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("JobPostingId")]
        public JobPosting? JobPosting { get; set; }
    }
}
