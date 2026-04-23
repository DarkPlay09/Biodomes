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

    public void OnGet()
    {
        EquipmentList = _repository.GetAll();

        Cards = EquipmentList.Select(e => new CatalogCardViewModel
        {
            Title = e.Name,
            ImagePath = e.ImagePath,
            Meta = new List<CatalogCardMetaItem>
            {
                new() { Label = "Produit", Value = e.ProducedElement?.ToString() ?? "-" },
                new() { Label = "Consomme", Value = e.ConsumedElement?.ToString() ?? "-" }
            },
            EditPage = "/Equipment/Edit",
            EditRouteValues = new Dictionary<string, string>
            {
                ["slug"] = e.Name
            },
            DeletePage = "/Equipment/Index",
            DeleteRouteValues = new Dictionary<string, string>
            {
                ["slug"] = e.Name
            }
        }).ToList();
    }

    public IActionResult OnPostDelete(string slug)
    {
        var equipment = _repository.GetBySlug(slug);
        _repository.DeleteBySlug(slug);
        _equipmentImageStorage.Delete(equipment?.ImagePath);
        return RedirectToPage();
    }
}
