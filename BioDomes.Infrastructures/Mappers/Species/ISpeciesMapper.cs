using BioDomes.Infrastructures.EntityFramework.Entities;

namespace BioDomes.Infrastructures.Mappers.Species;

public interface ISpeciesMapper
{
    Domains.Entities.Species ToDomain(SpeciesEntity entity);
    SpeciesEntity ToEntity(Domains.Entities.Species species, int creatorId);
    void UpdateEntity(SpeciesEntity target, Domains.Entities.Species source, int creatorId);
}
