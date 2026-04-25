using BioDomes.Infrastructures.EntityFramework.Entities;

namespace BioDomes.Infrastructures.Mappers.Equipment;

public interface IEquipmentMapper
{
    Domains.Entities.Equipment ToDomain(EquipmentEntity entity);
    EquipmentEntity ToEntity(Domains.Entities.Equipment equipment, int creatorId);
    void UpdateEntity(EquipmentEntity target, Domains.Entities.Equipment source, int creatorId);
}
