using BioDomes.Domains;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Species;

public class AddModel : PageModel
{
    private readonly ISpeciesRepository _repo;

    public AddModel(ISpeciesRepository repo)
    {
        _repo = repo;
    }

    [BindProperty] public SpeciesInputModel Input { get; set; } = new();

    [TempData] public string? LastInsertedSpeciesName { get; set; }

    public IEnumerable<SelectListItem> ClassificationOptions =>
        Enum.GetNames<SpeciesClassification>()
            .Select(x => new SelectListItem(x, x));

    public IEnumerable<SelectListItem> DietOptions =>
    Enum.GetNames<DietType>()
        .Select(x => new SelectListItem(x, x));

    public void OnGet()
    {
    }
    
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        if (!Enum.TryParse<SpeciesClassification>(Input.Classification, out var classification))
        {
            ModelState.AddModelError("Input.Classification", "Classification invalide.");
            return Page();
        }

        if (!Enum.TryParse<DietType>(Input.Diet, out var diet))
        {
            ModelState.AddModelError("Input.Diet", "Régime alimentaire invalide.");
            return Page();
        }
        
        var species = new Domains.Entities.Species(
            Input.Name!,
            classification,
            diet,
            Input.AdultSize,
            Input.Weight,
            Input.ImageUrl
            );
        
        try
        {
            _repo.Add(species);
        }
        catch (InvalidOperationException)
        {
            ModelState.AddModelError("Input.Name", "Une espèce avec ce nom existe déjà.");
            return Page();
        }

        LastInsertedSpeciesName = Input.Name;
        return RedirectToPage("/Species");
    }
}