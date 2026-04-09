namespace BioDomes.Domains.Repositories;

public interface ISpeciesImageStorage
{
    Task<string?> SaveAsync(string specieName, string originalFileName, Stream content);

    void Delete(string? imagePath);
}