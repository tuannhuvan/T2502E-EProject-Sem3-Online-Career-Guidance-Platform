
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models;
[Table("resources")]
public class Resource
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("path_id")]
    public int PathId { get; set; }

    [Column("resource_type")]
    public string? ResourceType { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("url")]
    public string? Url { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("created_by")]
    public string CreatedBy { get; set; } = "System";

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("updated_by")]
    public string? UpdatedBy { get; set; }

    [Column("status")]
    public int Status { get; set; } = 1;

    [ForeignKey("PathId")]
    public CareerPath? CareerPath { get; set; }
}