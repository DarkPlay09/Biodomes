using BioDomes.Domains;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages;

public class SpeciesModel : PageModel
{
    private readonly ISpeciesRepository _repository;
    
    public SpeciesModel(ISpeciesRepository repository)
    {
        _repository = repository;
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
        _repository.DeleteBySlug(slug);
        return RedirectToPage(); 
    }
}