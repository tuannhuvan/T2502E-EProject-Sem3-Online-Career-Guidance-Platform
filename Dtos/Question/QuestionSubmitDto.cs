namespace Career_Guidance_Platform.Dtos.Question;

public class QuestionSubmitDto
{
    public int QuestionId { get; set; }

    public int OptionId { get; set; }

    public int CurrentQuestion { get; set; }
    public int CareerTestId { get; set; }
}