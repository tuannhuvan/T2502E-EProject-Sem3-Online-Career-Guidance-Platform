using System.Collections.Generic;

namespace Career_Guidance_Platform.Models.ViewModels
{
    public class QuestionOptionDto
    {
        public string Content { get; set; } = string.Empty;
        public int? CareerPathId { get; set; }
        public int Weight { get; set; } = 1;
    }

    public class QuestionAdminDto
    {
        public int? Id { get; set; }
        public int TestId { get; set; }
        public string TestType { get; set; } = "Interests"; // Interests, Skills, Personality, Values
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = "Active"; // Active, Inactive
        public List<QuestionOptionDto> Options { get; set; } = new List<QuestionOptionDto>();
    }
}
