namespace Career_Guidance_Platform.Dtos;

public class AssessmentResultDto
{
    public string UserId { get; set; }  = null!;

    public int CareerTestId { get; set; }

    public List<UserAnswerDto> Answers { get; set; } = new();
}