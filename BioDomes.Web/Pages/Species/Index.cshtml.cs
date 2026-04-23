using System.Security.Claims;
using BioDomes.Domains.Repositories;
using BioDomes.Web.Pages.Shared.Cards;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Species;

public class SpeciesModel : PageModel
{
    private readonly ISpeciesRepository _repository;
    private readonly IWebHostEnvironment _environment;

    public SpeciesModel(ISpeciesRepository repository, IWebHostEnvironment environment)
    {
        _repository = repository;
        _environment = environment;
    }

    [TempData]
    public string? LastInsertedSpeciesName { get; set; }

    public IReadOnlyList<Domains.Entities.Species> SpeciesList { get; private set; } = new List<Domains.Entities.Species>();
    public IReadOnlyList<CatalogCardViewModel> Cards { get; private set; } = new List<CatalogCardViewModel>();

    public IActionResult OnGet()
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        SpeciesList = _repository.GetAll()
            .Where(s => s.IsPublicAvailable || s.Creator?.Id == currentUserId)
            .ToList();

        Cards = SpeciesList.Select(s =>
        {
            var isOwner = s.Creator?.Id == currentUserId;

            return new CatalogCardViewModel
            {
                Title = s.Name,
                ImagePath = s.ImagePath,
                Badge = s.Classification.ToString(),
                Meta = new List<CatalogCardMetaItem>
                {
                    new() { Label = "Régime", Value = s.Diet.ToString() },
                    new() { Label = "Poids", Value = $"{s.Weight} kg" }
                },
                EditPage = isOwner ? "/Species/Edit" : null,
                EditRouteValues = isOwner
                    ? new Dictionary<string, string> { ["slug"] = s.Name }
                    : null,
                DeletePage = isOwner ? "/Species/Index" : null,
                DeleteRouteValues = isOwner
                    ? new Dictionary<string, string> { ["slug"] = s.Name }
                    : null
            };
        }).ToList();

        return Page();
    }

    public IActionResult OnPostDelete(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var species = _repository.GetBySlug(slug);
        if (species is null)
            return NotFound();
        if (species.Creator?.Id != currentUserId)
            return Forbid();

        _repository.DeleteBySlug(slug);

        if (!string.IsNullOrWhiteSpace(species.ImagePath) &&
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

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}
