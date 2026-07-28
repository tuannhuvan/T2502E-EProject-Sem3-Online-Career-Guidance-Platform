namespace Career_Guidance_Platform.Dtos.Question;

public class QuestionTestDto
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public string QuestionTypeTitle { get; set; } = string.Empty;

    public List<QuestionOptionDto> Options { get; set; } = new();
}