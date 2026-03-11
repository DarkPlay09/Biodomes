namespace BioDomes.Domains.Entities;

public class Equipment
{
    public string Name { get; set; }
    public string Produces { get; set; }
    public string Consumes { get; set; }
    public string? ImageUrl { get; set; }
    public string? CreatedByUserName { get; set; }
    public bool IsPublic { get; set; }

    public Equipment(
        string name,
        string produces, // Temporairement en string
        string consumes, // Temporairement en string
        string? imageUrl = null,
        string? createdByUserName = null, // Temporairement en string
        bool isPublic = false)
    {
        Name = name;
        Produces = produces;
        Consumes = consumes;
        ImageUrl = imageUrl;
        CreatedByUserName = createdByUserName;
        IsPublic = isPublic;
    }
}