using BioDomes.Domains.Extensions;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace BioDomes.Infrastructures.Files;

public class SpeciesImageStorage : ISpeciesImageStorage
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    private static readonly string[] AvailableExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public SpeciesImageStorage(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<string?> SaveAsync(string specieName, string originalFileName, Stream content)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (!AvailableExtensions.Contains(extension))
            throw new InvalidOperationException("Image must be a [.jpg, .jpeg, .png, .webp] file.");

        var baseFileName = specieName.ToKebabCase();
        var uniquePart = Guid.NewGuid().ToString("N")[..8];
        var fileName = $"{baseFileName}-{uniquePart}{extension}";

        var uploadsRoot = GetUploadsRoot();

        var filePath = Path.Combine(
            uploadsRoot,
            "species",
            fileName
        );

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream);

        return $"~/uploads/species/{fileName}";
    }

    public void Delete(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return;

        var normalizedPath = imagePath
            .Replace("\\", "/")
            .Trim();

        if (normalizedPath.StartsWith("~/"))
            normalizedPath = normalizedPath[2..];

        normalizedPath = normalizedPath.TrimStart('/');

        if (!normalizedPath.StartsWith("uploads/species/"))
            return;

        var relativePath = normalizedPath["uploads/".Length..]
            .Replace('/', Path.DirectorySeparatorChar);

        var absolutePath = Path.Combine(GetUploadsRoot(), relativePath);

        if (File.Exists(absolutePath))
            File.Delete(absolutePath);
    }

    private string GetUploadsRoot()
    {
        var configuredPath = _configuration["Uploads:RootPath"];

        if (!string.IsNullOrWhiteSpace(configuredPath))
            return Path.GetFullPath(configuredPath);

        return Path.Combine(_environment.ContentRootPath, "uploads");
    }
}