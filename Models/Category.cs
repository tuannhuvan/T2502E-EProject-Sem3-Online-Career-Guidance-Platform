namespace Career_Guidance_Platform.Models;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<CareerPath> CareerPaths { get; set; }
        = new List<CareerPath>();
}