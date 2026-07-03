namespace Career_Guidance_Platform.Dtos.Question;

public class QuestionFullDto
{
    public int Id { get; set; }
    public int TestId { get; set; }
    
    public string TestTitle { get; set; } = string.Empty;
    public int QuestionTypeId { get; set; }
    
    public string QuestionTypeTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public List<OptionCreateDto> Options { get; set; } = new();
}