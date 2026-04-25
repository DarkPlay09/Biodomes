using System.Security.Claims;
using BioDomes.Domains.Repositories;
using BioDomes.Web.Pages.Shared.Cards;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Equipment;

/// <summary>
/// PageModel de la page listant les équipements accessibles à l'utilisateur connecté.
/// Un utilisateur voit les équipements publics ainsi que ses propres équipements privés.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IEquipmentRepository _repository;
    private readonly IEquipmentImageStorage _equipmentImageStorage;

    /// <summary>
    /// Initialise la page catalogue des équipements avec le repository et le service d'images.
    /// </summary>
    /// <param name="repository">Repository permettant de lire et supprimer les équipements.</param>
    /// <param name="equipmentImageStorage">Service permettant de supprimer l'image liée à un équipement.</param>
    public IndexModel(IEquipmentRepository repository, IEquipmentImageStorage equipmentImageStorage)
    {
        _repository = repository;
        _equipmentImageStorage = equipmentImageStorage;
    }

    /// <summary>
    /// Liste des équipements visibles par l'utilisateur courant.
    /// </summary>
    public IReadOnlyList<Domains.Entities.Equipment> EquipmentList { get; private set; } = new List<Domains.Entities.Equipment>();

    /// <summary>
    /// Liste des cartes prêtes à être affichées par la vue.
    /// </summary>
    public IReadOnlyList<CatalogCardViewModel> Cards { get; private set; } = new List<CatalogCardViewModel>();

    /// <summary>
    /// Charge les équipements visibles et les transforme en cartes d'affichage.
    /// </summary>
    /// <returns>La page catalogue ou une demande de connexion si l'utilisateur n'est pas identifié.</returns>
    public IActionResult OnGet()
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        // Un équipement est visible s'il est public ou s'il appartient à l'utilisateur connecté.
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

    /// <summary>
    /// Supprime un équipement appartenant à l'utilisateur connecté.
    /// L'image associée est également supprimée du stockage.
    /// </summary>
    /// <param name="slug">Slug de l'équipement à supprimer.</param>
    /// <returns>Une redirection vers la page courante ou une erreur si l'action n'est pas autorisée.</returns>
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

    /// <summary>
    /// Récupère l'identifiant de l'utilisateur connecté depuis les claims Identity.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur si la récupération réussit.</param>
    /// <returns>True si un identifiant valide a été trouvé, sinon false.</returns>
    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}