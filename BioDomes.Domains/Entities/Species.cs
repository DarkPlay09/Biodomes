using BioDomes.Domains.Enums;

namespace BioDomes.Domains.Entities;

public class Species
{
    
    public int Id { get; set; } // Primary key
    public string Name { get; set; }
    public SpeciesClassification Classification { get; set; }
    public DietType Diet { get; set; }
    public double AdultSize { get; set; }
    public double Weight { get; set; }
    public string? ImagePath { get; set; }
    public UserAccount? Creator { get; set; }
    public bool IsPublicAvailable { get; set; }
    
    public Species(
        string name,
        SpeciesClassification classification,
        DietType diet,
        double adultSize,
        double weight,
        string? imagePath = null,
        UserAccount? creator = null,
        bool isPublicAvailable = false)
    {
        Name = name;
        Classification = classification;
        Diet = diet;
        AdultSize = adultSize;
        Weight = weight;
        ImagePath = imagePath;
        Creator = creator;
        IsPublicAvailable = isPublicAvailable;
    }
}
