using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("faq_items")]
    public class FaqItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("question")]
        public string Question { get; set; } = string.Empty;

        [Required]
        [Column("answer")]
        public string Answer { get; set; } = string.Empty;

        [Required]
        [Column("category")]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
    }
}
