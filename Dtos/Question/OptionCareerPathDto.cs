using System.ComponentModel.DataAnnotations;

namespace Career_Guidance_Platform.Dtos.Question;

public class OptionCareerPathDto
{
    public int CareerPathId { get; set; }
    public string CareerPathTitle { get; set; } = string.Empty;
    public int Weight { get; set; }
}