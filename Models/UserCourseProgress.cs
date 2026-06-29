using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("user_course_progress")]
    public class UserCourseProgress
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int CourseId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DeadlineDate { get; set; }

        public int ProgressPercent { get; set; }

        // Learning / Completed / Expired
        public string Status { get; set; } = "Learning";

        public bool TestPassed { get; set; }

        public double TestScore { get; set; }

        public virtual User? User { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual CareerPathCourse? Course { get; set; }
    }
}