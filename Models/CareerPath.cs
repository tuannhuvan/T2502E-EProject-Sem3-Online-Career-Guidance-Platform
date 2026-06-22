namespace Career_Guidance_Platform.Models;

public class CareerPath
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Status { get; set; } = 1;

    // NAVIGATION
    public Category Category { get; set; } = null!;

    public ICollection<OptionCareerPath> OptionCareerPaths { get; set; }
        = new List<OptionCareerPath>();

    public ICollection<AssessmentResult> AssessmentResults { get; set; }
        = new List<AssessmentResult>();

    
   
}