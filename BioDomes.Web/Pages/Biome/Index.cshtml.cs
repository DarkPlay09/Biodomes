using System.Security.Claims;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Biome;

public class IndexModel : PageModel
{
    private readonly IBiomeRepository _biomeRepository;
    private readonly ISlugService _slugService;
    
    public sealed record BiomeRowVm(
        int Id,
        string Name,
        double Temperature,
        double AbsoluteHumidity,
        BiomeState State,
        string Slug
        );

    public List<BiomeRowVm> Rows { get; private set; } = [];

    public IndexModel(IBiomeRepository biomeRepository, ISlugService slugService)
    {
        _biomeRepository = biomeRepository;
        _slugService = slugService;
    }
    
    public IReadOnlyList<Domains.Entities.Biome> Biomes { get; set; } = [];
    public int TotalCount { get; private set; }
    public int OptimalCount { get; private set; }
    public int InstableCount { get; private set; }
    public int CritiqueCount { get; private set; }
    
    public IActionResult OnGet()
    {
        if (!TryGetCurrentUserId(out var userId))
            return Challenge();
        
        Biomes = _biomeRepository.GetAllByCreator(userId);
        
        TotalCount = Biomes.Count;
        OptimalCount = Biomes.Count(b => b.State == BiomeState.Optimal);
        InstableCount = Biomes.Count(b => b.State == BiomeState.Instable);
        CritiqueCount = Biomes.Count(b => b.State == BiomeState.Critique);

        Rows = Biomes
            .Select(b => new BiomeRowVm(
                b.Id,
                b.Name,
                b.Temperature,
                b.AbsoluteHumidity,
                b.State,
                _slugService.ToSlug(b.Name)))
            .ToList();
        
        return Page();
    }

    public IActionResult OnPostDelete(string slug)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Challenge();

        var biome = _biomeRepository.GetBySlug(slug);
        if (biome is null)
            return NotFound();

        if (biome.Creator.Id != userId)
            return Forbid();
        
        _biomeRepository.DeleteBySlug(slug);
        return RedirectToPage();
    }
    
    private bool TryGetCurrentUserId(out int userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId) && userId > 0;
    }
}