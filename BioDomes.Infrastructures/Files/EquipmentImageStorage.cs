using BioDomes.Domains.Extensions;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Hosting;

namespace BioDomes.Infrastructures.Files;

public class EquipmentImageStorage : IEquipmentImageStorage
{
    private readonly IWebHostEnvironment _environment;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public EquipmentImageStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveAsync(string equipmentName, string originalFileName, Stream content)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Image must be a [.jpg, .jpeg, .png, .webp] file.");

        var baseFileName = equipmentName.ToKebabCase();
        var uniquePart = Guid.NewGuid().ToString("N")[..8];
        var fileName = $"{baseFileName}-{uniquePart}{extension}";

        var filePath = Path.Combine(_environment.WebRootPath, "images", "equipment", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream);

        return $"/images/equipment/{fileName}";
    }

    public void Delete(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return;

        if (!imagePath.StartsWith("/images/equipment/"))
            return;

        var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);

        if (File.Exists(absolutePath))
            File.Delete(absolutePath);
    }
}
