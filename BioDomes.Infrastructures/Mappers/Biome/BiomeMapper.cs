using BioDomes.Domains.Entities;
using BioDomes.Infrastructures.EntityFramework.Entities;

namespace BioDomes.Infrastructures.Mappers.Biome;

public class BiomeMapper : IBiomeMapper
{
    public Domains.Entities.Biome ToDomain(BiomeEntity entity)
    {
        return new Domains.Entities.Biome(
            entity.Name,
            entity.Temperature,
            entity.AbsoluteHumidity,
            new UserAccount { Id = entity.CreatorId },
            entity.UpdatedAt
            )
        {
            Id = entity.Id
        };
    }

    public BiomeEntity ToEntity(Domains.Entities.Biome biome, int creatorId)
    {
        return new BiomeEntity
        {
            Name = biome.Name,
            Temperature = biome.Temperature,
            AbsoluteHumidity = biome.AbsoluteHumidity,
            State = biome.State.ToString(),
            CreatorId = creatorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateEntity(BiomeEntity target, Domains.Entities.Biome source, int creatorId)
    {
        target.Name = source.Name;
        target.Temperature = source.Temperature;
        target.AbsoluteHumidity = source.AbsoluteHumidity;
        target.State = source.State.ToString();
        target.CreatorId = creatorId;
        target.UpdatedAt = DateTime.UtcNow;
    }
}