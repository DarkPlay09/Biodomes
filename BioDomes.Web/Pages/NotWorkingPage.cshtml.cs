using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages;

public class NotWorkingPage : PageModel
{
    public void OnGet()
    {
        throw new InvalidOperationException("Congrats ! You reached the not working page in dev mode");
    }
}