using BioDomes.Domains.Enums;
namespace BioDomes.Domains.Entities;

public class Equipment
{
    public int Id { get; set; } // Primary key
    public string Name { get; set; }
    public ResourceType? ProducedElement { get; set; }
    public ResourceType? ConsumedElement { get; set; }
    public string? ImagePath { get; set; }
    public UserAccount Creator { get; set; }
    public bool IsPublicAvailable { get; set; }

    public Equipment(
        string name,
        ResourceType? producedElement,
        ResourceType? consumedElement,
        string? imagePath,
        UserAccount creator,
        bool isPublicAvailable = false)
    {
        if (producedElement is null &&
            consumedElement is null)
        {
            throw new ArgumentException("Un équipement doit produire et/ou consommer au moins un élément.");
        }
        Name = name;
        ProducedElement = producedElement;
        ConsumedElement = consumedElement;
        ImagePath = imagePath;
        Creator = creator ?? throw new ArgumentNullException(nameof(creator));
        IsPublicAvailable = isPublicAvailable;
    }
}
