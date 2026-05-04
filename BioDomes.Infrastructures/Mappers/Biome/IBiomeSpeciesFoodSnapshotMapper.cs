using BioDomes.Domains.Entities;
using BioDomes.Infrastructures.EntityFramework.Links;

namespace BioDomes.Infrastructures.Mappers.Biome;

public interface IBiomeSpeciesFoodSnapshotMapper
{
    IReadOnlyList<BiomeSpeciesFoodSnapshot> ToFoodSnapshots(IEnumerable<BiomeSpeciesLink> speciesLinks);
}
