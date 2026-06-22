namespace Career_Guidance_Platform.Models;

public class OptionCareerPath
{
    public int Id { get; set; }

    public int OptionId { get; set; }

    public int CareerPathId { get; set; }

    public int Weight { get; set; } = 1;

    public DateTime CreatedAt { get; set; }

    public int Status { get; set; } = 1;

    // NAVIGATION
    public QuestionOption QuestionOption { get; set; } = null!;

    public CareerPath CareerPath { get; set; } = null!;
}