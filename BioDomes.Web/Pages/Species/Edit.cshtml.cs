using BioDomes.Domains;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Species;

public class EditModel : PageModel
{
    private readonly ISpeciesRepository _repo;
    
    public EditModel(ISpeciesRepository repo) => _repo = repo;
    
    [BindProperty]
    public SpeciesInputModel Input { get; set; } = new();

    public IEnumerable<SelectListItem> ClassificationOptions { get; } =
        Enum.GetNames<SpeciesClassification>()
            .Select(x => new SelectListItem(x, x));

    public IEnumerable<SelectListItem> DietOptions =>
        Enum.GetNames<DietType>()
            .Select(x => new SelectListItem(x, x));
    
    public IActionResult OnGet(string slug)
    {
        var s = _repo.GetBySlug(slug);
        if (s is null) return NotFound();
        
        Input.Name = s.Name;
        Input.Classification = s.Classification.ToString();
        Input.Diet = s.Diet.ToString();
        Input.AdultSize = s.AdultSize;
        Input.Weight = s.Weight;
        Input.ImageUrl = s.ImageUrl;
        
        return Page();
    }

    public IActionResult OnPost(string slug)
    {
        if (!ModelState.IsValid)
            return Page();

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

        _repo.Update(slug, species);

        return RedirectToPage("/Species");
    }
}