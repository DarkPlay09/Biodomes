namespace BioDomes.Infrastructures.EntityFramework.Entities;

public class UserEntity
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string? AvatarPath { get; set; }
    public DateOnly BirthDate { get; set; }
    public string? ResearchOrganization { get; set; }

    public string Role { get; set; } = "User";

    public ICollection<BiomeEntity> CreatedBiomes { get; set; } = new List<BiomeEntity>();
    public ICollection<SpeciesEntity> CreatedSpecies { get; set; } = new List<SpeciesEntity>();
    public ICollection<EquipmentEntity> CreatedEquipments { get; set; } = new List<EquipmentEntity>();
}
