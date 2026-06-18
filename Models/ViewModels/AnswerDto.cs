using System.Collections.Generic;

namespace Career_Guidance_Platform.Models.ViewModels
{
    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public int OptionId { get; set; }
    }

    public class UserSubmissionsModel
    {
        public int TestId { get; set; }
        public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
    }
}