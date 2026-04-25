using System.Security.Claims;
using BioDomes.Domains.Repositories;
using BioDomes.Web.Pages.Shared.Cards;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Equipment;

public class IndexModel : PageModel
{
    private readonly IEquipmentRepository _repository;
    private readonly IEquipmentImageStorage _equipmentImageStorage;

    public IndexModel(IEquipmentRepository repository, IEquipmentImageStorage equipmentImageStorage)
    {
        _repository = repository;
        _equipmentImageStorage = equipmentImageStorage;
    }

    public IReadOnlyList<Domains.Entities.Equipment> EquipmentList { get; private set; } = new List<Domains.Entities.Equipment>();
    public IReadOnlyList<CatalogCardViewModel> Cards { get; private set; } = new List<CatalogCardViewModel>();

    public IActionResult OnGet()
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        EquipmentList = _repository.GetAll()
            .Where(e => e.IsPublicAvailable || e.Creator.Id == currentUserId)
            .ToList();

        Cards = EquipmentList.Select(e =>
        {
            var isOwner = e.Creator.Id == currentUserId;

            return new CatalogCardViewModel
            {
                Title = e.Name,
                ImagePath = e.ImagePath,
                Meta = new List<CatalogCardMetaItem>
                {
                    new() { Label = "Produit", Value = e.ProducedElement?.ToString() ?? "-" },
                    new() { Label = "Consomme", Value = e.ConsumedElement?.ToString() ?? "-" }
                },
                EditPage = isOwner ? "/Equipment/Edit" : null,
                EditRouteValues = isOwner
                    ? new Dictionary<string, string> { ["slug"] = e.Name }
                    : null,
                DeletePage = isOwner ? "/Equipment/Index" : null,
                DeleteRouteValues = isOwner
                    ? new Dictionary<string, string> { ["slug"] = e.Name }
                    : null
            };
        }).ToList();

        return Page();
    }

    public IActionResult OnPostDelete(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var equipment = _repository.GetBySlug(slug);
        if (equipment is null)
            return NotFound();
        if (equipment.Creator.Id != currentUserId)
            return Forbid();

        _repository.DeleteBySlug(slug);
        _equipmentImageStorage.Delete(equipment.ImagePath);
        return RedirectToPage();
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}
