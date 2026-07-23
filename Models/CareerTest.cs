namespace Career_Guidance_Platform.Models;

public class CareerTest
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Status { get; set; } = 1;

    public ICollection<Question> Questions { get; set; }
        = new List<Question>();

    public ICollection<AssessmentResult> AssessmentResults { get; set; }
        = new List<AssessmentResult>();
}