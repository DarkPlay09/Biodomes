using BioDomes.Web.ViewModels.Stats;

namespace BioDomes.Web.Services.Stats;

/// <summary>
/// Définit le contrat d'un service capable de construire les statistiques
/// affichées sur le tableau de bord d'un biome.
/// </summary>
public interface IStatsDashboardService
{
    /// <summary>
    /// Construit le tableau de bord statistique d'un biome appartenant à un utilisateur.
    /// </summary>
    /// <param name="userId">
    /// Identifiant de l'utilisateur connecté.
    /// </param>
    /// <param name="biomeSlug">
    /// Slug du biome à analyser.
    /// </param>
    /// <param name="period">
    /// Période sélectionnée pour le filtre d'affichage.
    /// Valeurs attendues : 1, 7, 30, 365 ou all.
    /// </param>
    /// <returns>
    /// Un ViewModel contenant les statistiques du biome si celui-ci existe,
    /// sinon null.
    /// </returns>
    Task<StatsDashboardVm?> BuildForBiomeAsync(int userId, string biomeSlug, string period);
}