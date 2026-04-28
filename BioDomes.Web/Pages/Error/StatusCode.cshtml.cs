using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Error;

/// <summary>
/// Page affichée lorsqu'une erreur HTTP spécifique est rencontrée.
/// Exemple : 404, 403, 500.
/// </summary>
public class StatusCodeModel : PageModel
{
    /// <summary>
    /// Code HTTP reçu.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int StatusCode { get; set; }

    /// <summary>
    /// Titre affiché selon le code HTTP.
    /// </summary>
    public string ErrorTitle { get; private set; } = "Erreur inconnue";

    /// <summary>
    /// Texte principal affiché selon le code HTTP.
    /// </summary>
    public string ErrorMessage { get; private set; } = "Une erreur inattendue est survenue.";

    /// <summary>
    /// Libellé du protocole affiché sur la carte.
    /// </summary>
    public string ProtocolLabel { get; private set; } = "Protocole système";
    
    /// <summary>
    /// Image principale affichée sur la page d'erreur.
    /// </summary>
    public string IllustrationPath { get; private set; } = "/images/error/error-default.png";

    /// <summary>
    /// Texte alternatif de l'image principale.
    /// </summary>
    public string IllustrationAlt { get; private set; } = "Illustration d'erreur système";
    
    public string StatusCssClass { get; private set; } = "error-status-page--default";
    
    /// <summary>
    /// URL sécurisée vers laquelle l'utilisateur peut revenir.
    /// </summary>
    public string SafeReturnUrl { get; private set; } = "/";

    /// <summary>
    /// Texte du bouton de retour.
    /// </summary>
    public string ReturnButtonLabel { get; private set; } = "Retour à l'accueil";

    /// <summary>
    /// Initialise la page d'erreur HTTP.
    /// </summary>
    /// <param name="statusCode">Code HTTP reçu dans l'URL.</param>
    public void OnGet(int statusCode)
    {
        StatusCode = statusCode;
        Response.StatusCode = statusCode;

        var referer = Request.Headers.Referer.ToString();

        SafeReturnUrl = GetFallbackReturnUrl();

        ReturnButtonLabel = User.Identity?.IsAuthenticated == true
            ? "Retour au tableau de bord"
            : "Retour à l'accueil";

        switch (statusCode)
        {
            case 403:
                ErrorTitle = "Accès refusé";
                ErrorMessage = "Vous n'avez pas les autorisations nécessaires pour accéder à ce secteur restreint.";
                ProtocolLabel = "Protocole 403";
                IllustrationPath = "/images/error/error-403.png";
                IllustrationAlt = "Illustration accès refusé";
                StatusCssClass = "error-status-page--forbidden";
                break;

            case 404:
                ErrorTitle = "Page introuvable";
                ErrorMessage = "Le secteur demandé n'existe pas ou a été déplacé dans la plateforme BioDomes.";
                ProtocolLabel = "Protocole 404";
                IllustrationPath = "/images/error/error-404.png";
                IllustrationAlt = "Illustration page introuvable";
                StatusCssClass = "error-status-page--not-found";
                break;

            default:
                ErrorTitle = $"Erreur {statusCode}";
                ErrorMessage = "Une anomalie a été détectée pendant le traitement de votre demande.";
                ProtocolLabel = $"Protocole {statusCode}";
                IllustrationPath = "/images/error/error-default.png";
                IllustrationAlt = "Illustration erreur système";
                StatusCssClass = "error-status-page--default";
                break;
        }
    }
    
    /// <summary>
    /// Retourne la page de retour selon l'état de connexion de l'utilisateur.
    /// </summary>
    /// <returns>Dashboard si connecté, accueil sinon.</returns>
    private string GetFallbackReturnUrl()
    {
        return User.Identity?.IsAuthenticated == true
            ? Url.Page("/Dashboard/Index") ?? "/Dashboard"
            : Url.Page("/Index") ?? "/";
    }
}