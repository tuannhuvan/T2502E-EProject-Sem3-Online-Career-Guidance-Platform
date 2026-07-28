namespace Career_Guidance_Platform.Dtos.Question;

public class QuestionFullDto
{
    public int Id { get; set; }
    public int CareerTestId { get; set; }
    
    public string CareerTestTitle { get; set; } = string.Empty;
    public int QuestionTypeId { get; set; }
    
    public string QuestionTypeTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public List<OptionCreateDto> Options { get; set; } = new();
}