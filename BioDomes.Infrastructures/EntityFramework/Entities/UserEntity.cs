using Microsoft.AspNetCore.Identity;

namespace BioDomes.Infrastructures.EntityFramework.Entities;

public class UserEntity : IdentityUser<int>
{
    public string? AvatarPath { get; set; }
    public DateOnly BirthDate { get; set; }
    public string? ResearchOrganization { get; set; }

    public string Role { get; set; } = "User";

    public ICollection<BiomeEntity> CreatedBiomes { get; set; } = new List<BiomeEntity>();
    public ICollection<SpeciesEntity> CreatedSpecies { get; set; } = new List<SpeciesEntity>();
    public ICollection<EquipmentEntity> CreatedEquipments { get; set; } = new List<EquipmentEntity>();
}
