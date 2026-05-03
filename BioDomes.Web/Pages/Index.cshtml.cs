using BioDomes.Domains.Queries.Biome.Home;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IBiomeRepository _biomeRepository;

    public IReadOnlyList<HomeBiomeCardDto> BestBiomes { get; private set; }
        = Array.Empty<HomeBiomeCardDto>();

    public IndexModel(
        ILogger<IndexModel> logger,
        IBiomeRepository biomeRepository)
    {
        _logger = logger;
        _biomeRepository = biomeRepository;
    }

    public void OnGet()
    {
        BestBiomes = _biomeRepository.GetBestBiomesForHome(2);
    }
}