using BioDomes.Domains.Repositories;
using BioDomes.Web.Pages.Shared.Cards;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Species;

public class SpeciesModel : PageModel
{
    private readonly ISpeciesRepository _repository;
    private readonly  IWebHostEnvironment _environment;
    
    public SpeciesModel(ISpeciesRepository repository, IWebHostEnvironment environment)
    {
        _repository = repository;
        _environment = environment;
    }
    
    [TempData]
    public string? LastInsertedSpeciesName { get; set; }

    public IReadOnlyList<Domains.Entities.Species> SpeciesList { get; private set; } = new List<Domains.Entities.Species>();
    public IReadOnlyList<CatalogCardViewModel> Cards { get; private set; } = new List<CatalogCardViewModel>();
    
    public void OnGet()
    {
        SpeciesList = _repository.GetAll();
        
        Cards = SpeciesList.Select(s => new CatalogCardViewModel
        {
            Title = s.Name,
            ImagePath = s.ImagePath,
            Badge = s.Classification.ToString(),
            Meta = new List<CatalogCardMetaItem>
            {
                new() { Label = "Régime", Value = s.Diet.ToString() },
                new() { Label = "Poids", Value = $"{s.Weight} kg" }
            },
            EditPage = "/Species/Edit",
            EditRouteValues = new Dictionary<string, string>
            {
                ["slug"] = s.Name
            },
            DeletePage = "/Species/Index",
            DeleteRouteValues = new Dictionary<string, string>
            {
                ["slug"] = s.Name
            }
        }).ToList();
    }
    
    public IActionResult OnPostDelete(string slug)
    {
        var species = _repository.GetBySlug(slug);
        _repository.DeleteBySlug(slug);
                
        if (!string.IsNullOrWhiteSpace(species?.ImagePath) &&
            species.ImagePath.StartsWith("/images/species/") &&
            species.ImagePath != "/images/species/noImageSpecie.png")
        {
            var relativePath = species.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);

            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }
        return RedirectToPage(); 
    }
}
