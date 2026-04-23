using BioDomes.Domains.Entities;

namespace BioDomes.Domains.Repositories;

public interface IEquipmentRepository
{
    IReadOnlyList<Equipment> GetAll();
    Equipment? GetBySlug(string slug);
    void Add(Equipment equipment);
    void Update(string slug, Equipment equipment);
    void DeleteBySlug(string slug);
}
