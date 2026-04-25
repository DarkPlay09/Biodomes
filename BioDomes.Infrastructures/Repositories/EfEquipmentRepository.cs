using BioDomes.Domains.Entities;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.Mappers.Equipment;
using BioDomes.Infrastructures.Services.Identity;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Infrastructures.Repositories;

public class EfEquipmentRepository : IEquipmentRepository
{
    private readonly BioDomesDbContext _context;
    private readonly IEquipmentMapper _equipmentMapper;
    private readonly IUserResolver _userResolver;
    private readonly ISlugService _slugService;

    public EfEquipmentRepository(
        BioDomesDbContext context,
        IEquipmentMapper equipmentMapper,
        IUserResolver userResolver,
        ISlugService slugService)
    {
        _context = context;
        _equipmentMapper = equipmentMapper;
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
            .Select(_equipmentMapper.ToDomain)
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

        return entity is null ? null : _equipmentMapper.ToDomain(entity);
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
        var entity = _equipmentMapper.ToEntity(equipment, creatorId);

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
        _equipmentMapper.UpdateEntity(entity, equipment, creatorId);

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
}
