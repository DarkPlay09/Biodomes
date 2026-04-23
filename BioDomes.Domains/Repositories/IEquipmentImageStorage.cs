namespace BioDomes.Domains.Repositories;

public interface IEquipmentImageStorage
{
    Task<string?> SaveAsync(string equipmentName, string originalFileName, Stream content);
    void Delete(string? imagePath);
}
