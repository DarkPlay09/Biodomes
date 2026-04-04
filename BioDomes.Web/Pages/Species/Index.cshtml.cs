using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages;

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
    
    public void OnGet()
    {
        SpeciesList = _repository.GetAll();
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