using System.Security.Claims;
using BioDomes.Domains;
using BioDomes.Domains.Entities;
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

    public string? CurrentImagePath { get; set; }

    public IActionResult OnGet(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var species = _repo.GetBySlug(slug);
        if (species is null)
            return NotFound();
        if (species.Creator?.Id != currentUserId)
            return Forbid();

        Input.Name = species.Name;
        Input.Classification = species.Classification.ToString();
        Input.Diet = species.Diet.ToString();
        Input.AdultSize = species.AdultSize;
        Input.Weight = species.Weight;
        CurrentImagePath = species.ImagePath;

        return Page();
    }

    public async Task<IActionResult> OnPost(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var existingSpecies = _repo.GetBySlug(slug);
        if (existingSpecies is null)
            return NotFound();
        if (existingSpecies.Creator?.Id != currentUserId)
            return Forbid();

        CurrentImagePath = existingSpecies.ImagePath;

        if (!ModelState.IsValid)
            return Page();

        var oldImagePath = existingSpecies.ImagePath;
        var imagePath = existingSpecies.ImagePath;
        var hasNewImage = false;

        if (Input.ImageFile is not null)
        {
            try
            {
                imagePath = await _speciesImageStorage.SaveAsync(
                    Input.Name!,
                    Input.ImageFile.FileName,
                    Input.ImageFile.OpenReadStream());
                hasNewImage = true;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Input.ImageFile", ex.Message);
                return Page();
            }
        }

        if (hasNewImage)
        {
            _speciesImageStorage.Delete(oldImagePath);
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
            imagePath,
            new UserAccount { Id = currentUserId },
            existingSpecies.IsPublicAvailable)
        {
            Id = existingSpecies.Id
        };

        _repo.Update(slug, species);

        return RedirectToPage("/Species/Index");
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}
