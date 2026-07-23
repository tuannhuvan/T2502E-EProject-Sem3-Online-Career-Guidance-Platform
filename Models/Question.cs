namespace Career_Guidance_Platform.Models;

public class Question
{
    public int Id { get; set; }

    public int CareerTestId  { get; set; }

    public int QuestionTypeId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public int Status { get; set; } = 1;

    // NAVIGATION
    public CareerTest CareerTest { get; set; } = null!;
    public QuestionType QuestionType { get; set; } = null!;

    public ICollection<QuestionOption> QuestionOptions { get; set; }
        = new List<QuestionOption>();
}