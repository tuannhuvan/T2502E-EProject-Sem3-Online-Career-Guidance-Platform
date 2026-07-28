namespace Career_Guidance_Platform.Dtos;

public class CareerResultDto
{
    public int CareerPathId { get; set; }

    public string CareerName { get; set; } = string.Empty;

    public decimal TotalScore { get; set; }

    public decimal Percentage { get; set; }
}