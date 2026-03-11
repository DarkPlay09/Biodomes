using BioDomes.Domains.Entities;

namespace BioDomes.Domains.Repositories;

public interface ISpeciesRepository
{
    IReadOnlyList<Species> GetAll();
    Species? GetBySlug(string slug);
    void Add(Species species);
    void Update(string slug, Species species);
    void DeleteBySlug(string slug);
}