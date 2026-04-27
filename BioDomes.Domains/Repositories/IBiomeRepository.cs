using BioDomes.Domains.Entities;

namespace BioDomes.Domains.Repositories;

public interface IBiomeRepository
{
    void Add(Biome biome);
    Biome? GetBySlug(string slug);
    IReadOnlyList<Biome> GetAllByCreator(int creatorId);
    void Update(string slug, Biome biome);
    void DeleteBySlug(string slug);
    int CountBiomesUsingSpecies(int speciesId);
}