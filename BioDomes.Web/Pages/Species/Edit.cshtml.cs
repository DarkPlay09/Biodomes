using BioDomes.Domains;
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

    public IActionResult OnGet(string slug)
    {
        var s = _repo.GetBySlug(slug);
        if (s is null) return NotFound();
        
        Input.Name = s.Name;
        Input.Type = s.Type;
        Input.Diet = s.Diet;
        Input.AdultSize = s.AdultSize;
        Input.ImageUrl = s.ImageUrl;
        
        return Page();
    }

    public IActionResult OnPost(string slug)
    {
        if (!ModelState.IsValid) return Page();
        
        _repo.Update(slug, Input.Name!, Input.Type!, Input.Diet!, Input.AdultSize, Input.ImageUrl);
        return RedirectToPage("/Species");
    }
}