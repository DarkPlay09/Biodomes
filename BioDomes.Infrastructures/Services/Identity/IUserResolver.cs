using BioDomes.Domains.Entities;

namespace BioDomes.Infrastructures.Services.Identity;

public interface IUserResolver
{
    int GetUserId(UserAccount creator);
}
