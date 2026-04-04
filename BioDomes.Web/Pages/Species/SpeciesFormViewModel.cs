using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Species;

public class SpeciesFormViewModel
{
    public SpeciesInputModel Input { get; set; } = new();
    public IEnumerable<SelectListItem> ClassificationOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> DietOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}