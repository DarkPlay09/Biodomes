using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    private readonly ILogger<ErrorModel> _logger;

    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);

    public int StatusCodeValue { get; set; }

    public string StatusCssClass => StatusCodeValue switch
    {
        403 => "error-status-page--forbidden",
        404 => "error-status-page--not-found",
        _ => "error-status-page--generic"
    };

    public string StatusTitle => StatusCodeValue switch
    {
        403 => "Accès refusé",
        404 => "Page introuvable",
        _ => "Erreur inattendue"
    };

    public string StatusText => StatusCodeValue switch
    {
        403 => "Vous n’avez pas les autorisations nécessaires pour accéder à ce secteur restreint. Revenez vers une zone sûre pour continuer votre exploration.",
        404 => "La ressource demandée est introuvable ou a peut-être été déplacée. Revenez au tableau de bord pour poursuivre votre navigation.",
        _ => "Une erreur est survenue pendant le traitement de votre demande. Veuillez réessayer plus tard."
    };

    public string StatusLabel => StatusCodeValue switch
    {
        403 => "Protocole 403",
        404 => "Protocole 404",
        _ => $"Protocole {StatusCodeValue}"
    };

    public void OnGet(int? statusCode = null)
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        var statusCodeFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        StatusCodeValue = statusCode
                          ?? HttpContext.Response.StatusCode;

        if (StatusCodeValue < 400)
        {
            StatusCodeValue = 500;
        }

        _logger.LogWarning(
            "Page d'erreur affichée. StatusCode: {StatusCode}. OriginalPath: {OriginalPath}. RequestId: {RequestId}",
            StatusCodeValue,
            statusCodeFeature?.OriginalPath,
            RequestId);
    }
}