using BioDomes.Domains;

namespace BioDomes.Infrastructures;

public interface ISpeciesRepository
{
    IReadOnlyList<Species> GetAll();
}

public class InMemorySpeciesRepository : ISpeciesRepository
{
    private readonly List<Species> _species = new()
    {
        new("Aloe vera", "Plante", "Photosynthèse", 0.8),
        new("Loup gris", "Mammifère", "Carnivore", 1.6),
        new("Raton laveur", "Mammifère", "Omnivore", 0.6),
    };
    
    public IReadOnlyList<Species> GetAll() =>  _species;
}