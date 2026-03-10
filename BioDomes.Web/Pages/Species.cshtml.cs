using BioDomes.Domains;
using BioDomes.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages;

public class SpeciesModel : PageModel
{
    [TempData]
    public string? LastInsertedSpeciesName { get; set; }
    
    private readonly ISpeciesRepository _repository;
    
    public IReadOnlyList<Domains.Species> Species { get; private set; } = new List<Domains.Species>();
    
    public SpeciesModel(ISpeciesRepository repository)
    {
        _repository = repository;
    }

    public void OnGet()
    {
        Species = _repository.GetAll();
    }
    
    public IActionResult OnPostDelete(string slug)
    {
        _repository.DeleteBySlug(slug);
        return RedirectToPage(); 
    }
}