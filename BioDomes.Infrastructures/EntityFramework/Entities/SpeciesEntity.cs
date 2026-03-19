namespace BioDomes.Infrastructures.EntityFramework.Entities;

public class SpeciesEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public string Diet { get; set; } = string.Empty;
    public double AdultSize { get; set; }
    public double Weight { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPublic { get; set; }
    public string? CreatedByUserName { get; set; }
}