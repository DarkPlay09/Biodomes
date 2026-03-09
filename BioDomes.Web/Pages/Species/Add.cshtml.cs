using BioDomes.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BioDomes.Web.Pages.Species;

public class AddModel : PageModel
{
    private readonly ISpeciesRepository _repo;

    public AddModel(ISpeciesRepository repo)
    {
        _repo = repo;
    }

    [BindProperty, Required]
    public string Name { get; set; } = "";

    [BindProperty, Required]
    public string Type { get; set; } = "";

    [BindProperty, Required]
    public string Diet { get; set; } = "";

    [BindProperty, Range(0.01, 999)]
    public double AdultSize { get; set; }

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        _repo.Add(Name, Type, Diet, AdultSize);   
        return RedirectToPage("/Species");       
    }
}