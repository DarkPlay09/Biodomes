using System.Security.Cryptography.X509Certificates;
using BioDomes.Domains;

namespace BioDomes.Infrastructures;

public interface ISpeciesRepository
{
    IReadOnlyList<Species> GetAll();
    Species? GetBySlug(string slug);
    void Add(string name, string type, string diet, double adultSize, string? imageUrl);
    void Update(string slug, string name, string type, string diet, double adultSize, string? imageUrl);
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
    
    public void Add(string name, string type, string diet, double adultSize, string? imageUrl)
    {
        if (_species.Any(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Nom d'éspèce duplique !");
        _species.Add(new Species(name, type, diet, adultSize, imageUrl));
    }

    public void Update(string slug, string name, string type, string diet, double adultSize, string? imageUrl)
    {
        var s = GetBySlug(slug);
        if (s is null) return;

        _species.Remove(s);
        _species.Add(new Species(name, type, diet, adultSize, imageUrl));
    }

    public void DeleteBySlug(string slug)
    {
        var s = GetBySlug(slug);
        if (s is null) return;
        _species.Remove(s);
    }
}