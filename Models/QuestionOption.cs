namespace Career_Guidance_Platform.Models;

public class QuestionOption
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public int Status { get; set; } = 1;

    // NAVIGATION
    public Question Question { get; set; } = null!;

    public ICollection<OptionCareerPath> OptionCareerPaths { get; set; }
        = new List<OptionCareerPath>();

  
}