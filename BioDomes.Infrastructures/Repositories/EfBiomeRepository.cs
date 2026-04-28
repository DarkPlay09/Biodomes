using BioDomes.Domains.Entities;
using BioDomes.Domains.Queries.Biome.Details;
using BioDomes.Domains.Queries.Biome.SelectSpecies;
using BioDomes.Domains.Queries.Biome.Species;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.EntityFramework.Links;
using BioDomes.Infrastructures.Mappers.Biome;
using BioDomes.Infrastructures.Services.Identity;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Infrastructures.Repositories;

public class EfBiomeRepository : IBiomeRepository
{
    private readonly IBiomeMapper _biomeMapper;
    private readonly BioDomesDbContext _context;
    private readonly ISlugService _slugService;
    private readonly IUserResolver _userResolver;

    public EfBiomeRepository(BioDomesDbContext context, IBiomeMapper biomeMapper, IUserResolver userResolver,
        ISlugService slugService)
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

    public BiomeDetailsDto? GetDetailsBySlugForCreator(string slug, int creatorId)
    {
        var normalizedSlug = _slugService.ToSlug(slug);

        var biome = _context.Biomes
            .AsNoTracking()
            .Where(b => b.CreatorId == creatorId)
            .ToList()
            .FirstOrDefault(b => _slugService.ToSlug(b.Name) == normalizedSlug);

        if (biome is null)
            return null;

        var species = _context.BiomeSpeciesLinks
            .AsNoTracking()
            .Where(link => link.BiomeId == biome.Id)
            .Include(link => link.Species)
            .OrderBy(link => link.Species.Name)
            .Select(link => new BiomeSpeciesItemDto
            {
                SpeciesId = link.SpeciesId,
                Name = link.Species.Name,
                Classification = link.Species.Classification,
                Diet = link.Species.Diet,
                IndividualCount = link.IndividualCount
            })
            .ToList();

        var equipmentLinks = _context.BiomeEquipmentLinks
            .AsNoTracking()
            .Where(link => link.BiomeId == biome.Id)
            .Include(link => link.Equipment)
            .OrderBy(link => link.Equipment.Name)
            .ToList();

        var equipments = equipmentLinks
            .Select(link => new BiomeEquipmentItemDto
            {
                EquipmentId = link.EquipmentId,
                Name = link.Equipment.Name,
                ImagePath = link.Equipment.ImagePath,
                ProducedElement = link.Equipment.ProducedElement?.ToString(),
                ConsumedElement = link.Equipment.ConsumedElement?.ToString()
            })
            .ToList();

        return new BiomeDetailsDto
        {
            Id = biome.Id,
            Slug = _slugService.ToSlug(biome.Name),
            Name = biome.Name,
            Temperature = biome.Temperature,
            AbsoluteHumidity = biome.AbsoluteHumidity,
            State = biome.State,
            UpdatedAt = biome.UpdatedAt,
            SpeciesCount = species.Count,
            EquipmentCount = equipments.Count,
            Species = species,
            Equipments = equipments
        };
    }

    public SelectSpeciesPageDto? GetSelectSpeciesPageData(string biomeSlug, int creatorId)
    {
        var normalizedSlug = _slugService.ToSlug(biomeSlug);

        var biome = _context.Biomes
            .AsNoTracking()
            .Where(b => b.CreatorId == creatorId)
            .ToList()
            .FirstOrDefault(b => _slugService.ToSlug(b.Name) == normalizedSlug);

        if (biome is null)
            return null;

        var linkedSpeciesIds = _context.BiomeSpeciesLinks
            .AsNoTracking()
            .Where(link => link.BiomeId == biome.Id)
            .Select(link => link.SpeciesId)
            .ToHashSet();

        var visibleSpecies = _context.Species
            .AsNoTracking()
            .Where(s => s.IsPublicAvailable || s.CreatorId == creatorId)
            .OrderBy(s => s.Name)
            .ToList();

        var cards = visibleSpecies
            .Select(s => new SelectSpeciesCardDto
            {
                SpeciesId = s.Id,
                Name = s.Name,
                Slug = _slugService.ToSlug(s.Name),
                ImagePath = string.IsNullOrWhiteSpace(s.ImagePath)
                    ? "/images/species/noImageSpecie.png"
                    : s.ImagePath,
                Classification = s.Classification,
                Diet = s.Diet,
                AdultSize = s.AdultSize,
                Weight = s.Weight,
                IsPublicAvailable = s.IsPublicAvailable,
                IsAlreadyInBiome = linkedSpeciesIds.Contains(s.Id)
            })
            .ToList();

        return new SelectSpeciesPageDto
        {
            BiomeId = biome.Id,
            BiomeName = biome.Name,
            BiomeSlug = _slugService.ToSlug(biome.Name),
            Species = cards
        };
    }


    public void AddSpeciesToBiome(int biomeId, IEnumerable<int> speciesIds, int countPerSpecies)
    {
        if (countPerSpecies <= 0)
            return;

        var biomeExists = _context.Biomes
            .AsNoTracking()
            .Any(b => b.Id == biomeId);

        if (!biomeExists)
            return;

        var ids = speciesIds?
            .Distinct()
            .ToList() ?? new List<int>();

        if (ids.Count == 0)
            return;

        var validSpeciesIds = _context.Species
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Select(s => s.Id)
            .ToHashSet();

        if (validSpeciesIds.Count == 0)
            return;

        var existingLinks = _context.BiomeSpeciesLinks
            .Where(link => link.BiomeId == biomeId && validSpeciesIds.Contains(link.SpeciesId))
            .ToList();

        var existingBySpeciesId = existingLinks.ToDictionary(link => link.SpeciesId, link => link);

        foreach (var id in validSpeciesIds)
            if (existingBySpeciesId.TryGetValue(id, out var link))
                link.IndividualCount += countPerSpecies;
            else
                _context.BiomeSpeciesLinks.Add(new BiomeSpeciesLink
                {
                    BiomeId = biomeId,
                    SpeciesId = id,
                    IndividualCount = countPerSpecies
                });

        _context.SaveChanges();
    }

    public BiomeSpeciesManagementPageDto? GetSpeciesManagementPageData(string biomeSlug, int creatorId,
        BiomeSpeciesManagementFiltersDto filters)
    {
        var normalizedSlug = _slugService.ToSlug(biomeSlug);

        var biome = _context.Biomes
            .AsNoTracking()
            .Where(b => b.CreatorId == creatorId)
            .ToList()
            .FirstOrDefault(b => _slugService.ToSlug(b.Name) == normalizedSlug);

        if (biome is null)
            return null;

        var speciesCards = _context.BiomeSpeciesLinks
            .AsNoTracking()
            .Where(link => link.BiomeId == biome.Id)
            .Include(link => link.Species)
            .Select(link => new BiomeSpeciesManagementCardDto
            {
                SpeciesId = link.SpeciesId,
                Name = link.Species.Name,
                Slug = _slugService.ToSlug(link.Species.Name),
                ImagePath = string.IsNullOrWhiteSpace(link.Species.ImagePath)
                    ? "/images/species/noImageSpecie.png"
                    : link.Species.ImagePath,
                Classification = link.Species.Classification,
                Diet = link.Species.Diet,
                AdultSize = link.Species.AdultSize,
                Weight = link.Species.Weight,
                CurrentIndividualCount = link.IndividualCount,
                IsPublicAvailable = link.Species.IsPublicAvailable
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.Trim();
            speciesCards = speciesCards
                .Where(card =>
                    card.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || card.Classification.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || card.Diet.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(filters.Classification))
        {
            var classification = filters.Classification.Trim();
            speciesCards = speciesCards
                .Where(card => string.Equals(card.Classification, classification, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(filters.Diet))
        {
            var diet = filters.Diet.Trim();
            speciesCards = speciesCards
                .Where(card => string.Equals(card.Diet, diet, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        speciesCards = speciesCards
            .OrderBy(card => card.Name)
            .ToList();

        return new BiomeSpeciesManagementPageDto
        {
            BiomeId = biome.Id,
            BiomeName = biome.Name,
            BiomeSlug = _slugService.ToSlug(biome.Name),
            SpeciesCards = speciesCards
        };
    }

    public void SetSpeciesCountInBiome(int biomeId, int speciesId, int individualCount)
    {
        var targetCount = Math.Max(0, individualCount);

        var existingLink = _context.BiomeSpeciesLinks
            .FirstOrDefault(link => link.BiomeId == biomeId && link.SpeciesId == speciesId);

        if (existingLink is null)
        {
            if (targetCount == 0)
                return;

            var biomeExists = _context.Biomes
                .AsNoTracking()
                .Any(b => b.Id == biomeId);

            if (!biomeExists)
                return;

            var speciesExists = _context.Species
                .AsNoTracking()
                .Any(s => s.Id == speciesId);

            if (!speciesExists)
                return;

            _context.BiomeSpeciesLinks.Add(new BiomeSpeciesLink
            {
                BiomeId = biomeId,
                SpeciesId = speciesId,
                IndividualCount = targetCount
            });
        }
        else
        {
            if (targetCount == 0)
                _context.BiomeSpeciesLinks.Remove(existingLink);
            else
                existingLink.IndividualCount = targetCount;
        }

        _context.SaveChanges();
    }
}