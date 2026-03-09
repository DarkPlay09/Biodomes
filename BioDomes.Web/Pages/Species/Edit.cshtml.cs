using BioDomes.Domains;
using BioDomes.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Species;

public class EditModel : PageModel
{
    private readonly ISpeciesRepository _repo;

    public Domains.Species? Species { get; private set; }

    public EditModel(ISpeciesRepository repo)
    {
        _repo = repo;
    }

    public IActionResult OnGet(string slug)
    {
        Species = _repo.GetBySlug(slug);

        if (Species is null)
            return NotFound();

        return Page();
    }
}