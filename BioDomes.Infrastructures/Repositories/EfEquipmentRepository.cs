using BioDomes.Domains.Entities;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.EntityFramework.Entities;
using BioDomes.Infrastructures.SpeciesMapper;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Infrastructures.Repositories;

public class EfEquipmentRepository : IEquipmentRepository
{
    private readonly BioDomesDbContext _context;
    private readonly IUserResolver _userResolver;
    private readonly ISpeciesSlugService _slugService;

    public EfEquipmentRepository(
        BioDomesDbContext context,
        IUserResolver userResolver,
        ISpeciesSlugService slugService)
    {
        _context = context;
        _userResolver = userResolver;
        _slugService = slugService;
    }

    public IReadOnlyList<Equipment> GetAll()
    {
        return _context.Equipments
            .AsNoTracking()
            .Include(e => e.Creator)
            .OrderBy(e => e.Name)
            .ToList()
            .Select(ToDomain)
            .ToList();
    }

    public Equipment? GetBySlug(string slug)
    {
        var normalizedSlug = _slugService.ToSlug(slug);

        var entity = _context.Equipments
            .AsNoTracking()
            .Include(e => e.Creator)
            .ToList()
            .FirstOrDefault(e => _slugService.ToSlug(e.Name) == normalizedSlug);

        return entity is null ? null : ToDomain(entity);
    }

    public void Add(Equipment equipment)
    {
        var normalizedName = _slugService.ToSlug(equipment.Name);

        var exists = _context.Equipments
            .AsNoTracking()
            .ToList()
            .Any(e => _slugService.ToSlug(e.Name) == normalizedName);

        if (exists)
            throw new InvalidOperationException("Un equipement avec ce nom existe deja.");

        var creatorId = _userResolver.GetUserId(equipment.Creator);
        var entity = ToEntity(equipment, creatorId);

        _context.Equipments.Add(entity);
        _context.SaveChanges();
    }

    public void Update(string slug, Equipment equipment)
    {
        var normalizedSlug = _slugService.ToSlug(slug);

        var entity = _context.Equipments
            .ToList()
            .FirstOrDefault(e => _slugService.ToSlug(e.Name) == normalizedSlug);

        if (entity is null)
            return;

        var creatorId = _userResolver.GetUserId(equipment.Creator);
        UpdateEntity(entity, equipment, creatorId);

        _context.SaveChanges();
    }

    public void DeleteBySlug(string slug)
    {
        var normalizedSlug = _slugService.ToSlug(slug);

        var entity = _context.Equipments
            .ToList()
            .FirstOrDefault(e => _slugService.ToSlug(e.Name) == normalizedSlug);

        if (entity is null)
            return;

        _context.Equipments.Remove(entity);
        _context.SaveChanges();
    }

    private static Equipment ToDomain(EquipmentEntity entity)
    {
        return new Equipment(
            entity.Name,
            entity.ProducedElement,
            entity.ConsumedElement,
            entity.ImagePath,
            entity.Creator is null
                ? null
                : new UserAccount
                {
                    Id = entity.Creator.Id,
                    UserName = entity.Creator.UserName ?? string.Empty,
                    Email = entity.Creator.Email ?? string.Empty
                },
            entity.IsPublicAvailable)
        {
            Id = entity.Id
        };
    }

    private static EquipmentEntity ToEntity(Equipment equipment, int creatorId)
    {
        return new EquipmentEntity
        {
            Name = equipment.Name,
            ProducedElement = equipment.ProducedElement,
            ConsumedElement = equipment.ConsumedElement,
            ImagePath = equipment.ImagePath,
            IsPublicAvailable = equipment.IsPublicAvailable,
            CreatorId = creatorId
        };
    }

    private static void UpdateEntity(EquipmentEntity entity, Equipment equipment, int creatorId)
    {
        entity.Name = equipment.Name;
        entity.ProducedElement = equipment.ProducedElement;
        entity.ConsumedElement = equipment.ConsumedElement;
        entity.ImagePath = equipment.ImagePath;
        entity.IsPublicAvailable = equipment.IsPublicAvailable;
        entity.CreatorId = creatorId;
    }
}
