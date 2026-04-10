using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BioDomes.Infrastructures.Identity;

/// <summary>
/// [Documentation générée par IA]
/// Initialise les données d'identité minimales au démarrage de l'application.
/// Cette classe seed un compte administrateur par défaut si celui-ci n'existe pas encore.
/// </summary>
public static class IdentityDataSeeder
{

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();

        const string adminUserName = "admin";
        const string adminEmail = "admin@biodomes.local";
        const string adminPassword = "Volauvent123";

        var admin = await userManager.Users
            .FirstOrDefaultAsync(u => u.Id == 1 || u.UserName == adminUserName);

        if (admin is null)
        {
            admin = new UserEntity
            {
                UserName = adminUserName,
                Email = adminEmail,
                BirthDate = new DateOnly(1990, 1, 1),
                ResearchOrganization = "Laudot Solutions"
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Admin seed failed (create): {errors}");
            }

            return;
        }

        // Met à jour l'admin existant sans le supprimer (les FK Species restent valides)
        admin.UserName = adminUserName;
        admin.Email = adminEmail;
        admin.NormalizedUserName = userManager.NormalizeName(adminUserName);
        admin.NormalizedEmail = userManager.NormalizeEmail(adminEmail);
        admin.BirthDate = new DateOnly(1990, 1, 1);
        admin.ResearchOrganization = "Laudot Solutions";
        admin.SecurityStamp = Guid.NewGuid().ToString();
        admin.ConcurrencyStamp = Guid.NewGuid().ToString();
        admin.PasswordHash = userManager.PasswordHasher.HashPassword(admin, adminPassword);

        var updateResult = await userManager.UpdateAsync(admin);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Admin seed failed (update): {errors}");
        }
    }
}
