using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Extensions;
using BioDomes.Domains.Repositories;

namespace BioDomes.Infrastructures;

public class InMemorySpeciesRepository : ISpeciesRepository
{
    private static readonly UserAccount InMemoryCreator = new() { Id = 1, UserName = "in-memory" };

    private readonly List<Species> _species = new()
    {
        new("Aloe vera", SpeciesClassification.Plantes, DietType.Photosynthese, 0.8, 15, null, InMemoryCreator),
        new("Loup gris", SpeciesClassification.Mammiferes, DietType.Carnivore, 1.6, 45000, null, InMemoryCreator),
        new("Raton laveur", SpeciesClassification.Mammiferes, DietType.Omnivore, 0.6, 9000, null, InMemoryCreator),
    };

    public IReadOnlyList<Species> GetAll() => _species;

    public Species? GetBySlug(string slug)
    {
        slug = (slug ?? "").Trim().ToLowerInvariant();

        return _species.FirstOrDefault(s => s.Name.ToKebabCase() == slug);
    }
    
    public void Add(Species species)
    {
        if (_species.Any(s => s.Name.Equals(species.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Nom d'espèce duplique !");
        _species.Add(species);
    }

    public void Update(string slug, Species species)
    {
        var s = GetBySlug(slug);
        if (s is null) return;

        _species.Remove(s);
        _species.Add(species);
    }

    public void DeleteBySlug(string slug)
    {
        var s = GetBySlug(slug);
        if (s is null) return;
        _species.Remove(s);
    }
}
