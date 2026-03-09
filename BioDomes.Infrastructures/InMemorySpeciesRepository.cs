using BioDomes.Domains;

namespace BioDomes.Infrastructures;

public interface ISpeciesRepository
{
    IReadOnlyList<Species> GetAll();
    Species? GetBySlug(string slug);
    void Add(string name, string type, string diet, double adultSize);
    void DeleteBySlug(string slug);
}

public class InMemorySpeciesRepository : ISpeciesRepository
{
    private readonly List<Species> _species = new()
    {
        new("Aloe vera", "Plante", "Photosynthèse", 0.8),
        new("Loup gris", "Mammifère", "Carnivore", 1.6),
        new("Raton laveur", "Mammifère", "Omnivore", 0.6),
    };

    public IReadOnlyList<Species> GetAll() => _species;

    public Species? GetBySlug(string slug)
    {
        slug = (slug ?? "").Trim().ToLowerInvariant();

        return _species.FirstOrDefault(s => s.Name.ToKebabCase() == slug);
    }
    
    public void Add(string name, string type, string diet, double adultSize)
    {
        _species.Add(new Species(name, type, diet, adultSize));
    }
    
    public void DeleteBySlug(string slug)
    {
        var s = GetBySlug(slug);
        if (s is null) return;
        _species.Remove(s);
    }
}