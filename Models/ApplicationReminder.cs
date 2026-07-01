using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("application_reminders")]
    public class ApplicationReminder
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("job_application_id")]
        public int? JobApplicationId { get; set; }

        [Column("reminder_date")]
        public DateTime ReminderDate { get; set; }

        [Required]
        [Column("message")]
        [StringLength(255)]
        public string Message { get; set; } = string.Empty;

        [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("JobApplicationId")]
        public JobApplication? JobApplication { get; set; }
    }
}
