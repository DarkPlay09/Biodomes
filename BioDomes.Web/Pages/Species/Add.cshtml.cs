using BioDomes.Domains;
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

    public void OnGet() { }

    [TempData]
    public string? LastInsertedSpeciesName { get; set; }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            _repo.Add(Input.Name!, Input.Type!, Input.Diet!, Input.AdultSize, Input.ImageUrl);
        }
        catch (InvalidOperationException)
        {
            ModelState.AddModelError("Input.Name", "Une espèce avec ce nom existe déjà.");
            return Page();
        }

        LastInsertedSpeciesName = Input.Name;
        return RedirectToPage("/Species");
    }

    public IEnumerable<SelectListItem> ClassificationOptions { get; } =
        Enum.GetNames<SpeciesClassification>()
            .Select(x => new SelectListItem(x, x));
}