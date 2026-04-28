using BioDomes.Domains.Entities;
using BioDomes.Infrastructures.EntityFramework.Entities;

namespace BioDomes.Infrastructures.Mappers.Equipment;

public class EquipmentMapper : IEquipmentMapper
{
    public Domains.Entities.Equipment ToDomain(EquipmentEntity entity)
    {
        return new Domains.Entities.Equipment(
            entity.Name,
            entity.ProducedElement,
            entity.ConsumedElement,
            entity.ImagePath,
            new UserAccount
            {
                Id = entity.CreatorId,
                UserName = entity.Creator?.UserName ?? string.Empty
            },
            entity.IsPublicAvailable)
        {
            Id = entity.Id
        };
    }

    public EquipmentEntity ToEntity(Domains.Entities.Equipment equipment, int creatorId)
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

    public void UpdateEntity(EquipmentEntity target, Domains.Entities.Equipment source, int creatorId)
    {
        target.Name = source.Name;
        target.ProducedElement = source.ProducedElement;
        target.ConsumedElement = source.ConsumedElement;
        target.ImagePath = source.ImagePath;
        target.IsPublicAvailable = source.IsPublicAvailable;
        target.CreatorId = creatorId;
    }
}
