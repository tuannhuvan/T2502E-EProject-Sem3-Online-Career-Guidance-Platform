namespace Career_Guidance_Platform.Models.ViewModels;

public class SaveResumeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentJson { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
}
