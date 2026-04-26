using System.Globalization;
using System.Security.Claims;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Biome;

public class DetailsModel : PageModel
{
    private readonly IBiomeRepository _repository;
    private readonly ISlugService _slugService;

    public DetailsModel(IBiomeRepository repository, ISlugService slugService)
    {
        _repository = repository;
        _slugService = slugService;
    }
    
    public BiomeDetailsViewModel? Details { get; private set; }


    public IActionResult OnGet(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();
        
        var biome = _repository.GetBySlug(slug);
        if (biome is null)
            return NotFound();
        
        if (biome.Creator.Id != currentUserId)
            return Forbid();
        
        InitializeViewModel(biome);
        
        return Page();
    }
    
    public IActionResult OnPostDelete(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var biome = _repository.GetBySlug(slug);

        if (biome is null)
        {
            return NotFound();
        }

        if (biome.Creator.Id != currentUserId)
        {
            return Forbid();
        }

        _repository.DeleteBySlug(slug);

        TempData["SuccessMessage"] = $"Le biome « {biome.Name} » a bien été supprimé.";

        return RedirectToPage("./Index");
    }

    private void InitializeViewModel(Domains.Entities.Biome biome)
    {
        var fr = CultureInfo.GetCultureInfo("fr-BE");

        Details = new BiomeDetailsViewModel
        {
            Id = biome.Id,
            Slug = _slugService.ToSlug(biome.Name),
            Name = biome.Name,
            TemperatureLabel = $"{biome.Temperature.ToString("0.0", fr)} °C",
            AbsoluteHumidityLabel = $"{biome.AbsoluteHumidity.ToString("0.00", fr)} g/m³",
            StateLabel = biome.State.ToString(),
            LastUpdatedLabel = biome.UpdatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm", fr),
            SpeciesCount = 0,   //TODO : remplacer par valeurs réelles
            EquipmentCount = 0  //TODO : remplacer par valeurs réelles
        };
    }
    
    //TODO : faire une classe pour cette méthode car elle est utilisée partout
    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}

public class BiomeDetailsViewModel
{
    public int Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;

    public string TemperatureLabel { get; init; } = string.Empty;
    public string AbsoluteHumidityLabel { get; init; } = string.Empty;
    public string StateLabel { get; init; } = string.Empty;
    public string LastUpdatedLabel { get; init; } = string.Empty;
    public int SpeciesCount { get; init; }
    public int EquipmentCount { get; init; }
}
