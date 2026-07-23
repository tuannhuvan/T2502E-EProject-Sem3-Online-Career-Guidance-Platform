namespace Career_Guidance_Platform.Models;

public class UserAnswer
{
    public int Id { get; set; }

    public int  AssessmentResultId { get; set; }
    
    public int QuestionId { get; set; }

    public int OptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public int Status { get; set; } = 1;

    // Navigation
    public AssessmentResult AssessmentResult { get; set; } = null!;

    public Question Question { get; set; } = null!;

    public QuestionOption Option { get; set; } = null!;
}