namespace Career_Guidance_Platform.Models;

public class QuestionType
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty; 
    // Text / SingleChoice / MultipleChoice
    public string? Description { get; set; }
    public ICollection<Question> Questions { get; set; }
        = new List<Question>();
}