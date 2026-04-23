using System.Security.Claims;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Species;

public class AddModel : PageModel
{
    private readonly ISpeciesRepository _repo;
    private readonly ISpeciesImageStorage _speciesImageStorage;

    public AddModel(ISpeciesRepository repo, ISpeciesImageStorage speciesImageStorage)
    {
        _repo = repo;
        _speciesImageStorage = speciesImageStorage;
    }

    [BindProperty]
    public SpeciesInputModel Input { get; set; } = new();

    [TempData]
    public string? LastInsertedSpeciesName { get; set; }

    public IEnumerable<SelectListItem> ClassificationOptions =>
        Enum.GetNames<SpeciesClassification>()
            .Select(x => new SelectListItem(x, x));

    public IEnumerable<SelectListItem> DietOptions =>
        Enum.GetNames<DietType>()
            .Select(x => new SelectListItem(x, x));

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        if (Input.ImageFile is null)
        {
            ModelState.AddModelError("Input.ImageFile", "Une image est requise.");
            return Page();
        }

        string? imagePath;
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
            imagePath,
            new UserAccount { Id = currentUserId },
            isPublicAvailable: false);

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
        return RedirectToPage("/Species/Index");
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}
