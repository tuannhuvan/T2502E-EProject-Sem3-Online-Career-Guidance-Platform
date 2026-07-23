using System.ComponentModel.DataAnnotations;

namespace Career_Guidance_Platform.Dtos.Question;

public class OptionCreateDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;

    public List<OptionCareerPathDto> CareerPaths { get; set; } = new();
}