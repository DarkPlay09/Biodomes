using BioDomes.Domains.Entities;

namespace BioDomes.Infrastructures.SpeciesMapper;

public interface IUserResolver
{
    int GetUserId(UserAccount? creator);
}
