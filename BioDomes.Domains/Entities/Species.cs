using BioDomes.Domains.Enums;

namespace BioDomes.Domains.Entities;

public class Species
{
    public string Name { get; set; }
    public SpeciesClassification Classification { get; set; }
    public DietType Diet { get; set; }
    public double AdultSize { get; set; }
    public double Weight { get; set; }
    public string? ImageUrl { get; set; }
    public string? CreatedByUserName { get; set; } // Temporairement en string
    public bool IsPublic { get; set; }
    
    public Species(
        string name,
        SpeciesClassification classification,
        DietType diet,
        double adultSize,
        double weight,
        string? imageUrl = null,
        string? createdByUserName = null,
        bool isPublic = false)
    {
        Name = name;
        Classification = classification;
        Diet = diet;
        AdultSize = adultSize;
        Weight = weight;
        ImageUrl = imageUrl;
        CreatedByUserName = createdByUserName;
        IsPublic = isPublic;
    }
}