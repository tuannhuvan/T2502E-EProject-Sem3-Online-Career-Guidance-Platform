using System.Collections.Generic;

namespace Career_Guidance_Platform.Models.ViewModels
{
    public class TakeTestViewModel
    {
        public int TestId { get; set; }
        public List<TakeTestQuestionVm> Questions { get; set; } = new List<TakeTestQuestionVm>();
    }

    public class TakeTestQuestionVm
    {
        public int QuestionId { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<TakeTestOptionVm> Options { get; set; } = new List<TakeTestOptionVm>();
    }

    public class TakeTestOptionVm
    {
        public int OptionId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}