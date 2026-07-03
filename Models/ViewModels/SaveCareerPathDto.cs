using System.Collections.Generic;

namespace Career_Guidance_Platform.Models.ViewModels;

public class SaveCareerPathDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public int? ParentPathId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public decimal SalaryMin { get; set; }
    public decimal SalaryMax { get; set; }
    public double JobOutlook { get; set; }
    public int Status { get; set; }

    public List<CareerPathSkillDto> Skills { get; set; } = new();
    public List<CareerStageDto> Stages { get; set; } = new();
}

public class CareerPathSkillDto
{
    public int SkillId { get; set; }
    public string ImportanceLevel { get; set; } = "Required";
}

public class CareerStageDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SequenceOrder { get; set; }
    public List<CareerStageSkillDto> Skills { get; set; } = new();
}

public class CareerStageSkillDto
{
    public int SkillId { get; set; }
    public string ProficiencyRequired { get; set; } = "Basic";
}
