using BioDomes.Domains.Entities;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.SpeciesMapper;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Infrastructures.Repositories;

public class EfSpeciesRepository : ISpeciesRepository
{
    private readonly BioDomesDbContext _context;
    private readonly ISpeciesMapper _speciesMapper;
    private readonly IUserResolver _userResolver;
    private readonly ISpeciesSlugService _slugService;

    public EfSpeciesRepository(
        BioDomesDbContext context,
        ISpeciesMapper speciesMapper,
        IUserResolver userResolver,
        ISpeciesSlugService slugService)
    {
        _context = context;
        _speciesMapper = speciesMapper;
        _userResolver = userResolver;
        _slugService = slugService;
    }

    public IReadOnlyList<Species> GetAll()
    {
        return _context.Species
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToList()
            .Select(_speciesMapper.ToDomain)
            .ToList();
    }

    public Species? GetBySlug(string slug)
    {
        var normalizedSlug = _slugService.ToSlug(slug);

        var entity = _context.Species
            .AsNoTracking()
            .ToList()
            .FirstOrDefault(s => _slugService.ToSlug(s.Name) == normalizedSlug);

        return entity is null ? null : _speciesMapper.ToDomain(entity);
    }

    public void Add(Species species)
    {
        var normalizedName = _slugService.ToSlug(species.Name);

        var exists = _context.Species
            .AsNoTracking()
            .ToList()
            .Any(s => _slugService.ToSlug(s.Name) == normalizedName);

        if (exists)
            throw new InvalidOperationException("Une espèce avec ce nom existe déjà.");

        var creatorId = _userResolver.GetUserId(species.Creator);
        var entity = _speciesMapper.ToEntity(species, creatorId);

        _context.Species.Add(entity);
        _context.SaveChanges();
    }

    public void Update(string slug, Species species)
    {
        var normalizedSlug = _slugService.ToSlug(slug);

        var entity = _context.Species
            .ToList()
            .FirstOrDefault(s => _slugService.ToSlug(s.Name) == normalizedSlug);

        if (entity is null)
            return;

        var creatorId = _userResolver.GetUserId(species.Creator);
        _speciesMapper.UpdateEntity(entity, species, creatorId);

        _context.SaveChanges();
    }

    public void DeleteBySlug(string slug)
    {
        var normalizedSlug = _slugService.ToSlug(slug);

        var entity = _context.Species
            .ToList()
            .FirstOrDefault(s => _slugService.ToSlug(s.Name) == normalizedSlug);

        if (entity is null)
            return;

        _context.Species.Remove(entity);
        _context.SaveChanges();
    }
}
