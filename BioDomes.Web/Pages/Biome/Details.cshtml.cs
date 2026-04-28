using System.Globalization;
using System.Security.Claims;
using BioDomes.Domains.Queries.Biome.Details;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Biome;

public class DetailsModel : PageModel
{
    private readonly IBiomeRepository _repository;

    public DetailsModel(IBiomeRepository repository)
    {
        _repository = repository;
    }
    
    public BiomeDetailsViewModel? Details { get; private set; }


    public IActionResult OnGet(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();
        
        var biomeDetails = _repository.GetDetailsBySlugForCreator(slug, currentUserId);
        if (biomeDetails is null)
            return NotFound();
        
        InitializeViewModel(biomeDetails);
        
        return Page();
    }
    
    public IActionResult OnPostDelete(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var biomeDetails = _repository.GetDetailsBySlugForCreator(slug, currentUserId);

        if (biomeDetails is null)
        {
            return NotFound();
        }

        _repository.DeleteBySlug(slug);

        TempData["SuccessMessage"] = $"Le biome « {biomeDetails.Name} » a bien été supprimé.";

        return RedirectToPage("./Index");
    }

    private void InitializeViewModel(BiomeDetailsDto detailsDto)
    {
        var fr = CultureInfo.GetCultureInfo("fr-BE");

        Details = new BiomeDetailsViewModel
        {
            Id = detailsDto.Id,
            Slug = detailsDto.Slug,
            Name = detailsDto.Name,
            TemperatureLabel = $"{detailsDto.Temperature.ToString("0.0", fr)} °C",
            AbsoluteHumidityLabel = $"{detailsDto.AbsoluteHumidity.ToString("0.00", fr)} g/m³",
            StateLabel = detailsDto.State,
            LastUpdatedLabel = detailsDto.UpdatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm", fr),
            SpeciesCount = detailsDto.SpeciesCount,
            EquipmentCount = detailsDto.EquipmentCount,
            TotalIndividuals = detailsDto.Species.Sum(s => s.IndividualCount),
            Species = detailsDto.Species
                .Select(s => new BiomeSpeciesRowVm
                {
                    SpeciesId = s.SpeciesId,
                    Name = s.Name,
                    Classification = s.Classification,
                    Diet = s.Diet,
                    IndividualCount = s.IndividualCount,
                    ImagePath = string.IsNullOrWhiteSpace(s.ImagePath) 
                        ? "/images/species/noImageSpecie.png"
                        : s.ImagePath
                })
                .ToList(),

            Equipments = detailsDto.Equipments
                .Select(e => new BiomeEquipmentRowVm
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    ProducedElement = e.ProducedElement ?? "-",
                    ConsumedElement = e.ConsumedElement ?? "-",
                    ImagePath = e.ImagePath
                })
                .ToList(),
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
    public int TotalIndividuals { get; init; }
    public int EquipmentCount { get; init; }
    public IReadOnlyList<BiomeSpeciesRowVm> Species { get; init; } = [];
    public IReadOnlyList<BiomeEquipmentRowVm> Equipments { get; init; } = [];
}

public class BiomeSpeciesRowVm
{
    public int SpeciesId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Classification { get; init; } = string.Empty;
    public string Diet { get; init; } = string.Empty;
    public int IndividualCount { get; init; }
    
    public string ImagePath { get; init; } = "/images/species/noImageSpecie.png";
}

public class BiomeEquipmentRowVm
{
    public int EquipmentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ProducedElement { get; init; } = string.Empty;
    public string ConsumedElement { get; init; } = string.Empty;
    public string? ImagePath { get; init; }
}