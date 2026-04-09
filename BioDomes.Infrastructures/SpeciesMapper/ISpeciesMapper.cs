using BioDomes.Domains.Entities;
using BioDomes.Infrastructures.EntityFramework.Entities;

namespace BioDomes.Infrastructures.SpeciesMapper;

public interface ISpeciesMapper
{
    Species ToDomain(SpeciesEntity entity);
    SpeciesEntity ToEntity(Species species, int creatorId);
    void UpdateEntity(SpeciesEntity target, Species source, int creatorId);
}
