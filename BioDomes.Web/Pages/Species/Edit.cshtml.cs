using BioDomes.Domains;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Species;

public class EditModel : PageModel
{
    private readonly ISpeciesRepository _repo;
    private readonly ISpeciesImageStorage _speciesImageStorage;

    public EditModel(ISpeciesRepository repo, ISpeciesImageStorage speciesImageStorage)
    {
        _repo = repo;
        _speciesImageStorage = speciesImageStorage;
    }

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

        return Page();
    }

    public async Task<IActionResult> OnPost(string slug)
    {
        // Supprime l'erreur de validation (image requise)
        ModelState.Remove("Input.ImageFile");

        if (!ModelState.IsValid)
            return Page();

        var existingSpecies = _repo.GetBySlug(slug);
        if (existingSpecies is null)
            return NotFound();

        var imagePath = existingSpecies.ImagePath;

        if (Input.ImageFile is not null)
        {
            try
            {
                imagePath = await _speciesImageStorage.SaveAsync(
                    Input.Name!,
                    Input.ImageFile.FileName,
                    Input.ImageFile.OpenReadStream());
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Input.ImageFile", ex.Message);
                return Page();
            }
        }

        if (!Enum.TryParse<SpeciesClassification>(Input.Classification, out var classification))
        {
            ModelState.AddModelError("Input.Classification", "Classification invalide.");
            return Page();
        }

        if (!Enum.TryParse<DietType>(Input.Diet, out var diet))
        {
            ModelState.AddModelError("Input.Diet", "Regime alimentaire invalide.");
            return Page();
        }

        var species = new Domains.Entities.Species(
            Input.Name!,
            classification,
            diet,
            Input.AdultSize,
            Input.Weight,
            imagePath
        );

        _repo.Update(slug, species);

        return RedirectToPage("/Species/Index");
    }
}
