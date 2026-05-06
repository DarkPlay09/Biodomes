using BioDomes.Infrastructures.EntityFramework.Entities;

namespace BioDomes.Infrastructures.Mappers.Biome;

public interface IBiomeMapper
{
    Domains.Entities.Biome ToDomain(BiomeEntity entity);
    BiomeEntity ToEntity(Domains.Entities.Biome biome, int creatorId);
    void UpdateEntity(BiomeEntity target, Domains.Entities.Biome source, int creatorId);
}