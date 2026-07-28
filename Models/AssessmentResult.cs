namespace Career_Guidance_Platform.Models;

public class AssessmentResult
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int CareerTestId  { get; set; }

    public int? RecommendedCareerPathId { get; set; }

    public decimal CompatibilityScore { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public int Status { get; set; } = 1;

    // NAVIGATION
    public User User { get; set; } = null!;
    public CareerTest CareerTest { get; set; } = null!;
    public CareerPath? RecommendedCareerPath { get; set; }

    public ICollection<UserAnswer> UserAnswers { get; set; }
        = new List<UserAnswer>();
    
}