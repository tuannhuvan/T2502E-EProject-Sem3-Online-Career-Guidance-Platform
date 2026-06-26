using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models;

[Table("resumes")]
public class Resume
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [Column("title")]
    [MaxLength(200)]
    public string Title { get; set; } = "CV không tên";

    [Required]
    [Column("content_json", TypeName = "longtext")]
    public string ContentJson { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }
}
