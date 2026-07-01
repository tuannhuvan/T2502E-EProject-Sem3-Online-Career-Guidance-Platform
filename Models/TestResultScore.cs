using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("test_result_scores")]
    public class TestResultScore
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("test_result_id")]
        public int TestResultId { get; set; }

        [Column("career_path_id")]
        public int CareerPathId { get; set; }

        [Column("score", TypeName = "decimal(5,2)")]
        public decimal Score { get; set; } = 0;

        [ForeignKey("TestResultId")]
        public TestResult? TestResult { get; set; }

        [ForeignKey("CareerPathId")]
        public CareerPath? CareerPath { get; set; }
    }
}
