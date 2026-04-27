using BioDomes.Domains.Entities;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.Mappers.Biome;
using BioDomes.Infrastructures.Services.Identity;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Infrastructures.Repositories;

public class EfBiomeRepository : IBiomeRepository
{
    private readonly BioDomesDbContext _context;
    private readonly IBiomeMapper _biomeMapper;
    private readonly IUserResolver _userResolver;
    private readonly ISlugService _slugService;

    public EfBiomeRepository(BioDomesDbContext context, IBiomeMapper biomeMapper, IUserResolver userResolver, ISlugService slugService)
    {
        _context = context;
        _biomeMapper = biomeMapper;
        _userResolver = userResolver;
        _slugService = slugService;
    }

    public void Add(Biome biome)
    {
        var normalizedName = _slugService.ToSlug(biome.Name);
        var creatorId = _userResolver.GetUserId(biome.Creator);
        
        var exists = _context.Biomes
            .AsNoTracking()
            .Where(b => b.CreatorId == creatorId)
            .ToList()
            .Any(b => _slugService.ToSlug(b.Name) == normalizedName);

        if (exists)
            throw new InvalidOperationException("Un biome avec ce nom existe déjà.");

        var entity = _biomeMapper.ToEntity(biome, creatorId);

        _context.Biomes.Add(entity);
        _context.SaveChanges();
    }

    public Biome? GetBySlug(string slug)
    {
        var normalizedSlug = _slugService.ToSlug(slug);

        var entity = _context.Biomes
            .AsNoTracking()
            .ToList()
            .FirstOrDefault(b => _slugService.ToSlug(b.Name) == normalizedSlug);

        return entity is null ? null : _biomeMapper.ToDomain(entity);
    }

    public IReadOnlyList<Biome> GetAllByCreator(int creatorId)
    {
        return _context.Biomes
            .AsNoTracking()
            .Where(b => b.CreatorId == creatorId)
            .OrderByDescending(b => b.UpdatedAt)
            .ToList()
            .Select(_biomeMapper.ToDomain)
            .ToList();
    }

    public void Update(string slug, Biome biome)
    {
        var normalizedSlug = _slugService.ToSlug(slug);

        var entity = _context.Biomes
            .ToList()
            .FirstOrDefault(s => _slugService.ToSlug(s.Name) == normalizedSlug);

        if (entity is null)
            return;

        var creatorId = _userResolver.GetUserId(biome.Creator);
        _biomeMapper.UpdateEntity(entity, biome, creatorId);

        _context.SaveChanges();
    }

    public void DeleteBySlug(string slug)
    {
        var normalizedSlug = _slugService.ToSlug(slug);

        var entity = _context.Biomes
            .ToList()
            .FirstOrDefault(b => _slugService.ToSlug(b.Name) == normalizedSlug);

        if (entity is null)
            return;

        _context.Biomes.Remove(entity);
        _context.SaveChanges();
    }
    
    public int CountBiomesUsingSpecies(int speciesId)
    {
        return _context.Biomes
            .AsNoTracking()
            .Count(biome => biome.BiomeSpeciesLinks.Any(link => link.SpeciesId == speciesId));
    }
    
    public int CountBiomesUsingEquipment(int equipmentId)
    {
        return _context.Biomes
            .AsNoTracking()
            .Count(biome => biome.BiomeEquipmentLinks.Any(link => link.EquipmentId == equipmentId));
    }
}