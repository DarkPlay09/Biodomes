using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Web.Pages.Biome;

public class SpeciesModel : PageModel
{
    private readonly IBiomeRepository _biomeRepository;
    private readonly BioDomesDbContext _context;
    private readonly ISlugService _slugService;

    public SpeciesModel(
        IBiomeRepository biomeRepository,
        BioDomesDbContext context,
        ISlugService slugService)
    {
        _biomeRepository = biomeRepository;
        _context = context;
        _slugService = slugService;
    }

    public BiomeSpeciesPageViewModel? PageData { get; private set; }

    [BindProperty]
    public AddSpeciesInputModel Input { get; set; } = new();

    public IActionResult OnGet(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var biome = _biomeRepository.GetBySlug(slug);
        if (biome is null)
            return NotFound();

        if (biome.Creator.Id != currentUserId)
            return Forbid();

        BuildPageData(biome.Id, biome.Name, currentUserId);

        return Page();
    }

    public IActionResult OnPostAdd(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var biome = _biomeRepository.GetBySlug(slug);
        if (biome is null)
            return NotFound();

        if (biome.Creator.Id != currentUserId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            BuildPageData(biome.Id, biome.Name, currentUserId);
            return Page();
        }

        var speciesEntity = _context.Species
            .AsNoTracking()
            .FirstOrDefault(s => s.Id == Input.SpeciesId);

        if (speciesEntity is null)
            return NotFound();

        var canUseSpecies = speciesEntity.IsPublicAvailable || speciesEntity.CreatorId == currentUserId;
        if (!canUseSpecies)
            return Forbid();

        var existingLink = _context.BiomeSpeciesLinks
            .FirstOrDefault(link => link.BiomeId == biome.Id && link.SpeciesId == Input.SpeciesId);

        if (existingLink is null)
        {
            _context.BiomeSpeciesLinks.Add(new()
            {
                BiomeId = biome.Id,
                SpeciesId = Input.SpeciesId,
                IndividualCount = Input.IndividualCount
            });
        }
        else
        {
            existingLink.IndividualCount += Input.IndividualCount;
        }

        _context.SaveChanges();
        TempData["SuccessMessage"] = "Espece ajoutee au biome.";

        return RedirectToPage(new { slug = _slugService.ToSlug(biome.Name) });
    }

    public IActionResult OnPostRemove(string slug, int speciesId)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var biome = _biomeRepository.GetBySlug(slug);
        if (biome is null)
            return NotFound();

        if (biome.Creator.Id != currentUserId)
            return Forbid();

        var link = _context.BiomeSpeciesLinks
            .FirstOrDefault(x => x.BiomeId == biome.Id && x.SpeciesId == speciesId);

        if (link is null)
            return NotFound();

        _context.BiomeSpeciesLinks.Remove(link);
        _context.SaveChanges();
        TempData["SuccessMessage"] = "Espece retiree du biome.";

        return RedirectToPage(new { slug = _slugService.ToSlug(biome.Name) });
    }

    private void BuildPageData(int biomeId, string biomeName, int currentUserId)
    {
        var linkedSpecies = _context.BiomeSpeciesLinks
            .AsNoTracking()
            .Where(link => link.BiomeId == biomeId)
            .Include(link => link.Species)
            .OrderBy(link => link.Species.Name)
            .Select(link => new BiomeLinkedSpeciesViewModel(
                link.SpeciesId,
                link.Species.Name,
                link.Species.Classification,
                link.Species.Diet,
                link.IndividualCount))
            .ToList();

        var availableSpecies = _context.Species
            .AsNoTracking()
            .Where(s => s.IsPublicAvailable || s.CreatorId == currentUserId)
            .OrderBy(s => s.Name)
            .Select(s => new SpeciesOptionViewModel(s.Id, s.Name))
            .ToList();

        PageData = new BiomeSpeciesPageViewModel(
            BiomeId: biomeId,
            BiomeName: biomeName,
            BiomeSlug: _slugService.ToSlug(biomeName),
            LinkedSpecies: linkedSpecies,
            AvailableSpecies: availableSpecies);
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId) && userId > 0;
    }
}

public sealed record BiomeSpeciesPageViewModel(
    int BiomeId,
    string BiomeName,
    string BiomeSlug,
    IReadOnlyList<BiomeLinkedSpeciesViewModel> LinkedSpecies,
    IReadOnlyList<SpeciesOptionViewModel> AvailableSpecies);

public sealed record BiomeLinkedSpeciesViewModel(
    int SpeciesId,
    string Name,
    string Classification,
    string Diet,
    int IndividualCount);

public sealed record SpeciesOptionViewModel(
    int Id,
    string Name);

public class AddSpeciesInputModel
{
    [Required]
    [Range(1, int.MaxValue)]
    public int SpeciesId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int IndividualCount { get; set; } = 1;
}
