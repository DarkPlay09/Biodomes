using BioDomes.Domains.Entities;
using BioDomes.Domains.Queries.Biome.Details;
using BioDomes.Domains.Queries.Biome.Home;
using BioDomes.Domains.Queries.Biome.SelectEquipment;
using BioDomes.Domains.Queries.Biome.SelectSpecies;
using BioDomes.Domains.Queries.Biome.Species;

namespace BioDomes.Domains.Repositories;

// TODO : diviser en plusieurs repository
public interface IBiomeRepository
{
    void Add(Biome biome);
    Biome? GetBySlug(string slug);
    IReadOnlyList<Biome> GetAllByCreator(int creatorId);
    void Update(string slug, Biome biome);
    void DeleteBySlug(string slug);
    int CountBiomesUsingSpecies(int speciesId);
    int CountBiomesUsingEquipment(int equipmentId);
    BiomeDetailsDto? GetDetailsBySlugForCreator(string slug, int creatorId); // TODO : inutilisé
    SelectSpeciesPageDto? GetSelectSpeciesPageData(string biomeSlug, int creatorId);
    void AddSpeciesToBiome(int biomeId, IEnumerable<int> speciesIds, int countPerSpecies);
    SelectEquipmentPageDto? GetSelectEquipmentPageData(string biomeSlug, int creatorId);
    void AddEquipmentToBiome(int biomeId, IEnumerable<int> equipmentIds);
    void RemoveEquipmentFromBiome(int biomeId, int equipmentId);
    BiomeSpeciesManagementPageDto? GetSpeciesManagementPageData(string biomeSlug, int creatorId, BiomeSpeciesManagementFiltersDto filters);
    void SetSpeciesCountInBiome(int biomeId, int speciesId, int individualCount);
    IReadOnlyList<HomeBiomeCardDto> GetBestBiomesForHome(int count);
}
