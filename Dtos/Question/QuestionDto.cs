namespace Career_Guidance_Platform.Dtos.Question;

public class QuestionDto
{
    public int Id { get; set; }
    public int CareerTestId { get; set; }
    public int QuestionTypeId { get; set; }
    public string Content { get; set; } = string.Empty;

    public List<OptionCreateDto> Options { get; set; } = new();
}