using BioDomes.Domains;
using BioDomes.Infrastructures;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages;

public class SpeciesModel : PageModel
{
    private readonly ISpeciesRepository _repository;
    
    public IReadOnlyList<Species> Species { get; private set; } = new List<Species>();
    
    public SpeciesModel(ISpeciesRepository repository)
    {
        _repository = repository;
    }

    public void OnGet()
    {
        Species = _repository.GetAll();
    }
}