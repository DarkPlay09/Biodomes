using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Equipment;

public class EquipmentFormViewModel
{
    public EquipmentInputModel Input { get; set; } = new();
    public IEnumerable<SelectListItem> ResourceOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}
