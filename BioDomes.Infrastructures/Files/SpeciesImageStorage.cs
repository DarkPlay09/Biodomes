using BioDomes.Domains.Extensions;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Hosting;

namespace BioDomes.Infrastructures.Files;

public class SpeciesImageStorage : ISpeciesImageStorage
{
    
    private readonly IWebHostEnvironment _environment;

    private static readonly string[] AvailableExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    
    public SpeciesImageStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveAsync(string specieName, string originalFileName, Stream content)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (!AvailableExtensions.Contains(extension))
            throw new InvalidOperationException("Image must be a [.jpg, .jpeg, .png, .webp] file.");

        var baseFileName = specieName.ToKebabCase();
        var uniquePart = Guid.NewGuid().ToString("N")[..8];
        var fileName = $"{baseFileName}-{uniquePart}{extension}";

        var filePath = Path.Combine(_environment.WebRootPath, "images", "species", fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using var fileStream = System.IO.File.Create(filePath);
        await content.CopyToAsync(fileStream);

        return $"/images/species/{fileName}";
    }
}
