using BioDomes.Domains.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Infrastructures.Services.Identity;

public class UserResolver : IUserResolver
{
    private readonly BioDomesDbContext _context;

    public UserResolver(BioDomesDbContext context)
    {
        _context = context;
    }

    public int GetUserId(UserAccount creator)
    {
        if (creator.Id <= 0)
            throw new InvalidOperationException("Creator.Id est obligatoire.");

        var exists = _context.Users
            .AsNoTracking()
            .Any(u => u.Id == creator.Id);

        return !exists 
            ? throw new InvalidOperationException($"Utilisateur introuvable pour Id={creator.Id}.") 
            : creator.Id;
    }

}
