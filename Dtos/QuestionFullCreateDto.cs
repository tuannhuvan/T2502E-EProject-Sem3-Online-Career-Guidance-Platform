namespace Career_Guidance_Platform.Dtos.Question;

public class QuestionFullCreateDto
{
    public int TestId  { get; set; }
    public int QuestionTypeId { get; set; }
    public string Content { get; set; } = string.Empty;

    public List<OptionCreateDto> Options { get; set; } = new();
}